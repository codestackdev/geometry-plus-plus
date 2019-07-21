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
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.SwEx.MacroFeature.Data;
using SolidWorks.Interop.swconst;

namespace CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet
{
    [Options("BodiesFillet", PROVIDER_MSG)]
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.fillet))]
    [SwEx.Common.Attributes.Title("Bodies Fillet")]
    [Description("Adds fillet to bodies in multi-body parts")]
    [SwEx.Common.Attributes.LoggerOptions(true, AddIn.LOGGER_NAME + ".BodiesFilletMacroFeature")]
    [ComVisible(true), ProgId(PROG_ID), Guid("A0856B88-393B-40BE-B523-B86D918CC91B")]
    public class BodiesFilletMacroFeature : GeometryMacroFeature<BodiesFilletFeatureDataModel, BodiesFilletPageDataModel>
    {
        private const int PREVIEW_CURVES_COUNT = 10;
        private const string TRACKING_DEFINITION_NAME = "__GeometryPlusPlus_FilletTracking__";

        private class BodiesFilletMacroFeatureRebuildResult : MacroFeatureRebuildBodyResult
        {
            internal IReadOnlyList<IFace2> FilletFaces { get; private set; }

            internal BodiesFilletMacroFeatureRebuildResult(IMacroFeatureData featData,
                IBody2[] bodies, IReadOnlyList<IFace2> filletFaces) : base(featData, true, bodies)
            {
                FilletFaces = filletFaces;
            }
        }

        public BodiesFilletMacroFeature() : base(System.Drawing.Color.Yellow)
        {
        }

        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.BodiesFilletMacroFeature";

        protected override MacroFeatureRebuildResult CreateRebuildResult(ISldWorks app, IMacroFeatureData featData, BodiesFilletFeatureDataModel parameters)
        {
            IReadOnlyList<IFace2> filletFaces;
            var bodies = CreateFillets(app, parameters.Selections, parameters.Radius, false, out filletFaces);
            
            return new BodiesFilletMacroFeatureRebuildResult(featData, bodies, filletFaces);
        }
        
        protected override void OnSetDimensions(ISldWorks app, IModelDoc2 model, IFeature feature, MacroFeatureRebuildResult rebuildResult,
            DimensionDataCollection dims, BodiesFilletFeatureDataModel parameters)
        {
            var filletRebuildRes = rebuildResult as BodiesFilletMacroFeatureRebuildResult;

            var filletFace = filletRebuildRes.FilletFaces.FirstOrDefault() as IFace2;

            if (filletFace != null)
            {
                var surf = filletFace.IGetSurface();

                var uvBounds = filletFace.GetUVBounds() as double[];
                var evalData = surf.Evaluate(
                    (uvBounds[0] + uvBounds[1]) / 2,
                    (uvBounds[2] + uvBounds[3]) / 2, 0, 0) as double[];

                var pt = new Point(evalData[0], evalData[1], evalData[2]);
                var norm = new Vector(evalData[evalData.Length - 3], evalData[evalData.Length - 2], evalData[evalData.Length - 1]);

                Vector axis = null;

                if (surf.IsCylinder())
                {
                    var cylParams = surf.CylinderParams as double[];
                    axis = new Vector(cylParams[3], cylParams[4], cylParams[5]);
                }
                else if (surf.IsTorus())
                {
                    var torParams = surf.TorusParams as double[];
                    axis = new Vector(torParams[3], torParams[4], torParams[5]);
                }

                var rad = parameters.Radius;

                var moveDir = new Vector(norm);
                moveDir.Scale(-1);

                var dimStartPt = pt.Move(moveDir, rad);

                dims[0].SetOrientation(dimStartPt, axis);
            }
        }
        
        protected override BodiesFilletFeatureDataModel ConvertPageToParams(BodiesFilletPageDataModel page)
        {
            var data = new BodiesFilletFeatureDataModel();
            data.EditBodies = GetBodies(page.Selections);
            data.Selections = page.Selections;
            data.Radius = page.Radius;
            return data;
        }

        protected override BodiesFilletPageDataModel ConvertParamsToPage(BodiesFilletFeatureDataModel parameters)
        {
            var data = new BodiesFilletPageDataModel();
            data.Radius = parameters.Radius;
            data.Selections = parameters.Selections;

            return data;
        }

        protected override IBody2[] CreatePreview(ISldWorks app, BodiesFilletFeatureDataModel parameters, ref IBody2[] editBodies)
        {
            editBodies = null;

            IReadOnlyList<IFace2> filletFaces;
            CreateFillets(app, parameters.Selections, parameters.Radius, true, out filletFaces);

            var previewBodies = new List<IBody2>();

            if (filletFaces != null)
            {
                foreach (var face in filletFaces)
                {
                    var surf = face.IGetSurface();

                    foreach (var curve in SplitFaceOnIsoCurves(face, PREVIEW_CURVES_COUNT, surf.IsCylinder()))
                    {
                        var body = curve.CreateWireBody();

                        previewBodies.Add(body);
                    }
                    
                    var edges = face.GetEdges() as object[];

                    if (edges != null)
                    {
                        previewBodies.AddRange(edges.Select(e => (e as IEdge).CreateWireBody()));
                    }
                }
            }

            return previewBodies.ToArray();
        }

        private static ICurve[] SplitFaceOnIsoCurves(IFace2 face, int curvesCount, bool vOrU)
        {
            var curves = new List<ICurve>();

            var surf = face.IGetSurface();

            var uvBounds = face.GetUVBounds() as double[];
            var minU = uvBounds[0];
            var maxU = uvBounds[1];
            var minV = uvBounds[2];
            var maxV = uvBounds[3];

            double thisMin;
            double thisMax;
            double otherMin;
            double otherMax;

            if (vOrU) //if v param
            {
                thisMin = minV;
                thisMax = maxV;
                otherMin = minU;
                otherMax = maxU;
            }
            else //if u param
            {
                thisMin = minU;
                thisMax = maxU;
                otherMin = minV;
                otherMax = maxV;
            }

            var step = (thisMax - thisMin) / (curvesCount - 1);

            for (int i = 1; i < curvesCount - 1; i++)
            {
                var par = thisMin + i * step;
                var curve = surf.MakeIsoCurve2(vOrU, ref par);

                double u;
                double v;

                if (vOrU)
                {
                    u = otherMin;
                    v = par;
                }
                else
                {
                    u = par;
                    v = otherMin;
                }

                var pt = surf.Evaluate(u, v, 0, 0) as double[];

                var startPt = new double[] { pt[0], pt[1], pt[2] };

                if (vOrU)
                {
                    u = otherMax;
                    v = par;
                }
                else
                {
                    u = par;
                    v = otherMax;
                }

                pt = surf.Evaluate(u, v, 0, 0) as double[];

                var endPt = new double[] { pt[0], pt[1], pt[2] };

                curve = curve.CreateTrimmedCurve2(startPt[0], startPt[1], startPt[2], endPt[0], endPt[1], endPt[2]);

                curves.Add(curve);
            }

            return curves.ToArray();
        }

        protected override IBody2[] CreateGeometry(ISldWorks app, BodiesFilletFeatureDataModel parameters)
        {
            IReadOnlyList<IFace2> filletFaces;
            return CreateFillets(app, parameters.Selections, parameters.Radius, false, out filletFaces);
        }

        private IBody2[] CreateFillets(ISldWorks app, List<object> dispatches, double radius, bool isPreview, out IReadOnlyList<IFace2> filletFaces)
        {
            if (dispatches == null)
            {
                throw new UserErrorException("Select entities to add fillets to");
            }

            if (radius <= 0)
            {
                throw new UserErrorException("Specify radius more than zero");
            }

            var bodies = new List<IBody2>();
            var createdFaces = new List<IFace2>();

            var entGroups = dispatches.GroupBy(d => GetBody(d));

            var trackCookie = -1;

            if (isPreview)
            {
                trackCookie = app.RegisterTrackingDefinition(TRACKING_DEFINITION_NAME);
            }
            
            foreach (var entGroup in entGroups)
            {
                var edges = new List<IEdge>();

                var body = entGroup.Key;

                foreach (var ent in entGroup)
                {
                    object[] entEdges = null;

                    if (ent is IBody2)
                    {
                        entEdges = (ent as IBody2).GetEdges() as object[];
                    }
                    else if (ent is IFace2)
                    {
                        entEdges = (ent as IFace2).GetEdges() as object[];
                    }
                    else if (ent is IEdge)
                    {
                        entEdges = new object[] { ent as IEdge };
                    }
                    else if (ent is IVertex)
                    {
                        entEdges = (ent as IVertex).GetEdges() as object[];
                    }

                    if (entEdges != null)
                    {
                        edges.AddRange(entEdges.Cast<IEdge>().Except(edges));
                    }
                }

                if (isPreview)
                {
                    CreateBodyForPreview(trackCookie, ref edges, ref body);
                }

                if (body.GetType() != (int)swBodyType_e.swSolidBody)
                {
                    throw new UserErrorException("Fillet can only be added to solid bodies");
                }

                var faces = body.AddConstantFillets(radius, edges.ToArray()) as object[];

                if (faces != null && faces.Any())
                {
                    createdFaces.AddRange(faces.Cast<IFace2>());
                }
                else
                {
                    throw new UserErrorException("Failed to create fillet for specified entities due to geometrical conditions");
                }

                bodies.Add(body);
            }

            filletFaces = createdFaces;

            return bodies.ToArray();
        }

        private void CreateBodyForPreview(int trackCookie, ref List<IEdge> edges, ref IBody2 body)
        {
            var inputEdges = edges;

            try
            {
                for (int i = 0; i < inputEdges.Count; i++)
                {
                    var status = (swTrackingIDError_e)inputEdges[i].SetTrackingID(trackCookie, i);
                    if (status != swTrackingIDError_e.swTrackingIDError_NoError)
                    {
                        Logger.Log($"Failed to set tracking id: {status}");
                        throw new UserErrorException("Failed to track entity");
                    }
                }

                body = body.ICopy();

                var copiedEdges = body.GetEdges() as object[];

                if (copiedEdges != null)
                {
                    var trackedEdges = copiedEdges.Where(e =>
                    {
                        object trackIds;
                        if ((e as IEdge).GetTrackingIDs(trackCookie, out trackIds)
                            == (int)swTrackingIDError_e.swTrackingIDError_NoError)
                        {
                            if (trackIds != null && (trackIds as int[]).Length == 1)
                            {
                                if ((trackIds as int[]).First() < inputEdges.Count)
                                {
                                    return true;
                                }
                            }

                            return true;
                        }

                        return false;
                    });

                    if (trackedEdges.Count() == inputEdges.Count)
                    {
                        edges = trackedEdges.Cast<IEdge>().ToList();
                    }
                    else
                    {
                        throw new UserErrorException("Failed to track entity");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
            finally
            {
                inputEdges.ForEach(e =>
                {
                    var status = (swTrackingIDError_e)e.RemoveTrackingID(trackCookie);
                    if (status != swTrackingIDError_e.swTrackingIDError_NoError)
                    {
                        Logger.Log($"Failed to remove tracking id: {status}");
                        throw new UserErrorException("Failed to track entity");
                    }
                });
            }
        }

        private List<IBody2> GetBodies(List<object> dispatches)
        {
            var bodies = new List<IBody2>();

            foreach (var disp in dispatches)
            {
                var body = GetBody(disp);

                if (!bodies.Contains(body))
                {
                    bodies.Add(body);
                }
            }

            return bodies;
        }

        private static IBody2 GetBody(object disp)
        {
            IBody2 body = null;

            if (disp is IBody2)
            {
                body = disp as IBody2;
            }
            else if (disp is IFace2)
            {
                body = (disp as IFace2).IGetBody();
            }
            else if (disp is IEdge)
            {
                body = (disp as IEdge).GetBody();
            }
            else if (disp is IVertex)
            {
                body = (((disp as IVertex).GetEdges() as object[]).First() as IEdge).GetBody();
            }

            return body;
        }
    }
}
