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
using SolidWorks.Interop.swconst;
using System.ComponentModel;
using CodeStack.SwEx.MacroFeature.Core;
using System.Diagnostics;

namespace CodeStack.Community.GeometryPlusPlus.Features.TrimSurfacesByRegion
{
    [ComVisible(true), ProgId(PROG_ID), Guid("16ABB9D1-887E-4FD1-BE67-847977261E73")]
    public class TrimSurfacesByRegionMacroFeature : ObsoleteMacroFeatureEx<CropBodies.CropBodiesMacroFeature>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.TrimSurfacesByRegionMacroFeature";
    }
}

namespace CodeStack.Community.GeometryPlusPlus.Features.CropBodies
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.crop_bodies))]
    [SwEx.Common.Attributes.Title("Crop Bodies")]
    [Description("Crops bodies with the selected profile")]
    [Options("CropBody", PROVIDER_MSG)]
    [SwEx.Common.Attributes.LoggerOptions(true, AddIn.LOGGER_NAME + ".CropBodiesMacroFeature")]
    [ComVisible(true), ProgId(PROG_ID), Guid("EEFD9EC5-77B1-4709-9550-C07FEEA4643A")]
    public class CropBodiesMacroFeature : GeometryMacroFeature<CropBodiesDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.CropGeometryMacroFeature";
        
        protected override IBody2[] CreateGeometry(ISldWorks app, CropBodiesDataModel parameters)
        {
            return CropGeometry(app.IGetModeler(), app.IGetMathUtility(),
                parameters.TargetBodies, parameters.TrimTools);
        }

        private IBody2[] CropGeometry(IModeler modeler, IMathUtility mathUtils,
            List<IBody2> targetBodies, List<object> trimTools)
        {   
            if (targetBodies == null || !targetBodies.Any())
            {
                throw new UserErrorException("Select target bodies to trim");
            }

            if (trimTools == null || !trimTools.Any())
            {
                throw new UserErrorException("Select trim tools (sketches or sketch regions)");
            }

            var resBodies = new List<IBody2>();

            var toolBodies = CreateToolBodies(modeler, mathUtils, trimTools);

            if (!toolBodies.Any())
            {
                throw new UserErrorException("No closed regions found in the selected trim tools");
            }

            foreach (var inputBody in targetBodies)
            {
                if (inputBody == null)
                {
                    continue; //TODO: investigate why first body is null
                }

                foreach (var solidBody in toolBodies)
                {
                    var targetBody = inputBody.ICopy();
                    
                    var toolBody = solidBody.ICopy();

                    int err;
                    object[] res = targetBody.Operations2((int)swBodyOperationType_e.SWBODYINTERSECT, toolBody, out err) as object[];

                    if (res != null)
                    {
                        foreach (IBody2 resBody in res)
                        {
                            resBodies.Add(resBody);
                        }
                    }
                }
            }

            return resBodies.ToArray();
        }

        private IBody2[] CreateToolBodies(IModeler modeler, IMathUtility mathUtils, List<object> tools)
        {
            var toolBodies = new List<IBody2>();

            foreach (var tool in tools)
            {
                if (tool is IFeature)
                {
                    var sketch = (tool as IFeature).GetSpecificFeature2() as ISketch;

                    var sketchContours = sketch.GetSketchContours() as object[];

                    if (sketchContours != null)
                    {
                        foreach (ISketchContour skCont in sketchContours)
                        {
                            toolBodies.Add(CreateBodyFromSketchContour(modeler, mathUtils, skCont));
                        }
                    }
                }

                if (tool is ISketchContour)
                {
                    toolBodies.Add(CreateBodyFromSketchContour(modeler, mathUtils, tool as ISketchContour));
                }
            }

            return toolBodies.ToArray();
        }

        //TODO: calculate height based on bounding box
        private IBody2 CreateBodyFromSketchContour(IModeler modeler,
            IMathUtility mathUtils, ISketchContour skCont, double height = 1000)
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

            var surf = modeler.CreatePlanarSurface2(centerPt.ArrayData, dirVec.ArrayData, refVec.ArrayData) as ISurface;

            var sheetBody = surf.CreateTrimmedSheet4(boundary, true) as Body2;

            if (sheetBody == null)
            {
                throw new NullReferenceException("Failed to create trimmed sheet from surface region");
            }

            var firstBody = modeler.CreateExtrudedBody(sheetBody, dirVec as MathVector, height * 0.9 / 2) as IBody2;
            var secondBody = modeler.CreateExtrudedBody(sheetBody, dirVec.IScale(-1), height * 0.9 / 2) as IBody2;

            int err;
            var res = firstBody.Operations2((int)swBodyOperationType_e.SWBODYADD, secondBody, out err) as object[];

            return res.First() as IBody2;
        }
    }
}
