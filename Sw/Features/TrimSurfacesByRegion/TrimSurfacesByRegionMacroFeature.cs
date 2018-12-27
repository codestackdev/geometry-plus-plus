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

namespace CodeStack.Community.GeometryPlusPlus.Features.TrimSurfacesByRegion
{

    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.trim_surface_region))]
    [Options("TrimSurface")]
    [ComVisible(true)]
    [ProgId(PROG_ID)]
    [Guid("16ABB9D1-887E-4FD1-BE67-847977261E73")]
    public class TrimSurfacesByRegionMacroFeature : GeometryMacroFeature<TrimSurfacesByRegionDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.TrimSurfacesByRegionMacroFeature";

        protected override IBody2[] CreateGeometry(ISldWorks app, TrimSurfacesByRegionDataModel parameters)
        {   
            if (parameters.TargetBodies == null || !parameters.TargetBodies.Any())
            {
                throw new UserErrorException("Select target bodies to trim");
            }

            if (parameters.TrimTools == null || !parameters.TrimTools.Any())
            {
                throw new UserErrorException("Select trim tools (sketches or sketch regions)");
            }

            var resBodies = new List<IBody2>();

            var modeler = app.IGetModeler();
            var mathUtils = app.IGetMathUtility();

            var toolBodies = CreateToolBodies(modeler, mathUtils, parameters.TrimTools);

            if (!toolBodies.Any())
            {
                throw new UserErrorException("No closed regions found in the selected trim tools");
            }

            foreach (var surfBody in parameters.TargetBodies)
            {
                if (surfBody == null)
                {
                    continue; //TODO: investigate why first body is null
                }

                foreach (var solidBody in toolBodies)
                {
                    var targetBody = surfBody.ICopy();
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

                    var sketchRegions = sketch.GetSketchRegions() as object[];

                    if (sketchRegions != null)
                    {
                        foreach (ISketchRegion skReg in sketchRegions)
                        {
                            toolBodies.Add(CreateBodyFromSketchRegion(modeler, mathUtils, skReg));
                        }
                    }
                }

                if (tool is ISketchRegion)
                {
                    toolBodies.Add(CreateBodyFromSketchRegion(modeler, mathUtils, tool as ISketchRegion));
                }

            }

            return toolBodies.ToArray();
        }

        private IBody2 CreateBodyFromSketchRegion(IModeler modeler,
            IMathUtility mathUtils, ISketchRegion skReg, double height = 1000)
        {
            var sketch = skReg.Sketch;

            if (sketch.Is3D())
            {
                throw new UserErrorException("Only 2D sketches are supported");
            }

            var transform = sketch.ModelToSketchTransform.IInverse();

            var boundary = (skReg.GetEdges() as object[])
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
