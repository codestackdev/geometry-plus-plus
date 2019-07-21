//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

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
    [Help("https://www.codestack.net/labs/solidworks/geometry-plus-plus/user-guide/extrude-surface-cap/")]
    public class ExtrudeSurfaceCapDataModel
    {
        [SelectionBox(swSelectType_e.swSelSKETCHES)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectProfile)]
        [Description("Extrusion profile")]
        [ControlOptions(height: 60)]
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
