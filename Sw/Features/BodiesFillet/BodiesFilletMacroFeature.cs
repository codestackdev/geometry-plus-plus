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

namespace CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet
{
    [Options("BodiesFillet")]
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.fillet))]
    [SwEx.Common.Attributes.Title("Bodies Fillet")]
    [Description("Adds fillet to bodies in multi-body parts")]
    [ComVisible(true), ProgId(PROG_ID), Guid("A0856B88-393B-40BE-B523-B86D918CC91B")]
    public class BodiesFilletMacroFeature : GeometryMacroFeature<BodiesFilletFeatureDataModel, BodiesFilletPageDataModel>
    {
        private class BodiesFilletMacroFeatureRebuildResult : MacroFeatureRebuildBodyResult
        {
            internal IReadOnlyList<IFace2> FilletFaces { get; private set; }

            internal BodiesFilletMacroFeatureRebuildResult(IMacroFeatureData featData,
                IBody2[] bodies, IReadOnlyList<IFace2> filletFaces) : base(featData, true, bodies)
            {
                FilletFaces = filletFaces;
            }
        }

        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.BodiesFilletMacroFeature";

        protected override MacroFeatureRebuildResult CreateRebuildResult(ISldWorks app, IMacroFeatureData featData, BodiesFilletFeatureDataModel parameters)
        {
            IReadOnlyList<IFace2> filletFaces;
            var bodies = CreateFillets(parameters.Selections, parameters.Radius, false, out filletFaces);

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

                var cylParams = surf.CylinderParams as double[];
                var axis = new Vector(cylParams[3], cylParams[4], cylParams[5]);
                var rad = cylParams[6];

                var moveDir = new Vector(norm);
                moveDir.Scale(-1);

                var dimStartPt = pt.Move(moveDir, rad);

                var dimVec = norm.Cross(axis);

                dims[0].SetDirection(dimStartPt, dimVec);
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
            IReadOnlyList<IFace2> filletFaces;
            return CreateFillets(parameters.Selections, parameters.Radius, true, out filletFaces);
        }

        protected override IBody2[] CreateGeometry(ISldWorks app, BodiesFilletFeatureDataModel parameters)
        {
            IReadOnlyList<IFace2> filletFaces;
            return CreateFillets(parameters.Selections, parameters.Radius, false, out filletFaces);
        }

        private IBody2[] CreateFillets(List<object> dispatches, double radius, bool isPreview, out IReadOnlyList<IFace2> filletFaces)
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

            foreach (var entGroup in entGroups)
            {
                var edges = new List<IEdge>();

                var body = entGroup.Key;

                if (isPreview)
                {
                    body = body.ICopy();
                    //TODO: add tracking entities to recognize the edges
                }

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

                var faces = body.AddConstantFillets(radius, edges.ToArray()) as object[];

                if (faces != null)
                {
                    createdFaces.AddRange(faces.Cast<IFace2>());
                }

                bodies.Add(body);
            }

            filletFaces = createdFaces;

            return bodies.ToArray();
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
