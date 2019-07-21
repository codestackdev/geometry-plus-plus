//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.MacroFeature.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using CodeStack.Community.GeometryPlusPlus.Exceptions;
using System.ComponentModel;
using CodeStack.SwEx.MacroFeature.Data;
using CodeStack.SwEx.MacroFeature.Base;

namespace CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.extrude_surface_caps))]
    [SwEx.Common.Attributes.Title("Extrude Surface Cap")]
    [Description("Creates extruded surface with caps")]
    [Options("ExtrudeSurfaceCap", PROVIDER_MSG)]
    [SwEx.Common.Attributes.LoggerOptions(true, AddIn.LOGGER_NAME + ".ExtrudeSurfaceCapMacroFeature")]
    [ComVisible(true), ProgId(PROG_ID), Guid("39FD6EA8-3113-40BA-A1D6-405CFF3931BF")]
    public class ExtrudeSurfaceCapMacroFeature : GeometryMacroFeature<ExtrudeSurfaceCapDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.ExtrudeSurfaceCapMacroFeature";

        protected override IBody2[] CreateGeometry(ISldWorks app, ExtrudeSurfaceCapDataModel parameters)
        {
            if (parameters.Profiles != null && parameters.Profiles.Any())
            {
                var bodies = new List<IBody2>();

                foreach (var profile in parameters.Profiles)
                {
                    if (profile is IFeature)
                    {
                        var sketch = (profile as IFeature).GetSpecificFeature2() as ISketch;

                        var sketchContours = sketch.GetSketchContours() as object[];

                        if (sketchContours != null)
                        {
                            foreach (ISketchContour skCont in sketchContours)
                            {
                                bodies.Add(CreateBodyFromSketchContour(app.IGetModeler(),
                                    app.IGetMathUtility(), skCont, parameters.Height, parameters.MidPlane));
                            }
                        }
                    }

                    if (profile is ISketchContour)
                    {
                        bodies.Add(CreateBodyFromSketchContour(app.IGetModeler(),
                                    app.IGetMathUtility(), profile as ISketchContour,
                                    parameters.Height, parameters.MidPlane));
                    }
                }

                return bodies.ToArray();
            }
            else
            {
                throw new UserErrorException("Please select the profile for extrusion");
            }
        }

        private IBody2 CreateBodyFromSketchContour(IModeler modeler,
            IMathUtility mathUtils, ISketchContour skCont, double height, bool midPlane)
        {
            var sketch = skCont.Sketch;

            if (sketch.Is3D())
            {
                throw new UserErrorException("Only 2D sketches are supported");
            }

            var transform = sketch.ModelToSketchTransform.IInverse();

            var boundary = (skCont.GetEdges() as object[])
                .Cast<IEdge>()
                .Select(e =>
                {
                    var curve = e.IGetCurve().ICopy();//must copy curve otherwise CreateTrimmedSheet4 is failing
                    return curve;
                }).ToArray();

            var centerPt = mathUtils.CreatePoint(new double[] { 0, 0, 0 }) as IMathPoint;
            var dirVec = mathUtils.CreateVector(new double[] { 0, 0, 1 }) as IMathVector;
            var refVec = mathUtils.CreateVector(new double[] { 1, 0, 0 }) as IMathVector;

            centerPt = centerPt.IMultiplyTransform(transform);
            dirVec = dirVec.IMultiplyTransform(transform);
            refVec = refVec.IMultiplyTransform(transform);

            if (midPlane)
            {
                var dirRevVec = new Vector(dirVec.ArrayData as double[]);
                dirRevVec.Scale(-1);

                var origPt = new Point(centerPt.ArrayData as double[]);

                MoveCurves(origPt, dirRevVec, height / 2, boundary, mathUtils);
                centerPt = mathUtils.CreatePoint(origPt.Move(dirRevVec, height / 2).ToArray()) as IMathPoint;
            }

            var surf = modeler.CreatePlanarSurface2(centerPt.ArrayData, dirVec.ArrayData, refVec.ArrayData) as ISurface;

            var sheetBody = surf.CreateTrimmedSheet4(boundary, true) as Body2;

            if (sheetBody == null)
            {
                throw new NullReferenceException("Failed to create trimmed sheet from surface region");
            }

            var solidBody = modeler.CreateExtrudedBody(sheetBody, dirVec as MathVector, height) as IBody2;

            var faces = (solidBody.GetFaces() as object[]).Cast<IFace2>().ToArray();

            return modeler.CreateSheetFromFaces(faces);
        }

        private void MoveCurves(Point fromPt, Vector dir, double dist, ICurve[] curves, IMathUtility mathUtils)
        {
            var newPt = fromPt.Move(dir, dist);

            var translation = fromPt - newPt;
            
            var maxtrix = new double[]
            {
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, translation.X, translation.Y, translation.Z,
                    1, 0, 0, 0
            };

            var transform = mathUtils.CreateTransform(maxtrix) as MathTransform;

            for (int i = 0; i < curves.Length; i++)
            {
                curves[i].ApplyTransform(transform);
            }
        }
    }
}