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
    [Message("Select surface to extrude with caps", "Extrude Surface With Caps")]
    public class ExtrudeSurfaceCapDataModel
    {
        [SelectionBox(swSelectType_e.swSelSKETCHES)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectProfile)]
        [Description("Extrusion profile")]
        [ParameterSelection]
        public List<object> Profiles { get; set; }

        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_Depth)]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.01,
                false, 0.1, 0.005, swPropMgrPageNumberBoxStyle_e.swPropMgrPageNumberBoxStyle_Thumbwheel)]
        [Description("Height of the extrusion")]
        [ParameterDimension(swDimensionType_e.swLinearDimension)]
        public double Height { get; set; } = 0.01;

        [DisplayName("Mid plane")]
        [Description("Extrude mid plane")]
        public bool MidPlane { get; set; }
    }
}
