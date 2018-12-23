using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.PMPage.Attributes;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap
{
    [PageOptions(typeof(Resources), nameof(Resources.extrude_surface_caps),
    swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
    | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton)]
    [Message("Select surface to extrude with caps", "Extrude Surface With Caps")]
    [DisplayName("Extrude Surface With Caps")]
    public class ExtrudeSurfaceCapDataModel
    {
        [SelectionBox(swSelectType_e.swSelSKETCHES, swSelectType_e.swSelSKETCHREGION)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectProfile)]
        [Description("Extrusion profile")]
        [ParameterSelection]
        public object Profile { get; set; }

        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_Depth)]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.001,
                true, 0.01, 0.0005, swPropMgrPageNumberBoxStyle_e.swPropMgrPageNumberBoxStyle_Thumbwheel)]
        [Description("Height of the extrusion")]
        [ParameterDimension(swDimensionType_e.swLinearDimension)]
        public double Height { get; set; }

        [DisplayName("Mid plane")]
        [Description("Extrude mid plane")]
        public bool MidPlane { get; set; }
    }
}
