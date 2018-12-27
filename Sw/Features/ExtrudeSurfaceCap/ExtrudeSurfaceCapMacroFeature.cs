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

namespace CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.extrude_surface_caps))]
    [Options("ExtrudeSurfaceCap")]
    [ComVisible(true)]
    [ProgId(PROG_ID)]
    [Guid("39FD6EA8-3113-40BA-A1D6-405CFF3931BF")]
    public class ExtrudeSurfaceCapMacroFeature : GeometryMacroFeature<ExtrudeSurfaceCapDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.ExtrudeSurfaceCapMacroFeature";

        protected override IBody2[] CreateGeometry(ISldWorks app, ExtrudeSurfaceCapDataModel parameters)
        {
            throw new UserErrorException("Not implemented");
        }
    }
}