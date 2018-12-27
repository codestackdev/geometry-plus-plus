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

namespace CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.fillet))]
    [Options("BodiesFillet")]
    [ComVisible(true)]
    [ProgId(PROG_ID)]
    [Guid("A0856B88-393B-40BE-B523-B86D918CC91B")]
    public class BodiesFilletMacroFeature : GeometryMacroFeature<BodiesFilletFeatureDataModel, BodiesFilletPageDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.BodiesFilletMacroFeature";

        protected override BodiesFilletFeatureDataModel ConvertPageToParams(BodiesFilletPageDataModel page)
        {
            var data = new BodiesFilletFeatureDataModel();
            data.EditBodies = null; //TODO: get all bodies from selection
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

        protected override IBody2[] CreateGeometry(ISldWorks app, BodiesFilletFeatureDataModel parameters)
        {
            throw new UserErrorException("Not implemented");
        }
    }
}
