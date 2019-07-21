//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.PMPage.Attributes;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus.Features.CropBodies
{
    [Message("Trims selected surface bodies with selected region", "Trim Surfaces By Region")]
    [Help("https://www.codestack.net/labs/solidworks/geometry-plus-plus/user-guide/crop-bodies/")]
    public class CropBodiesDataModel
    {
        [SelectionBox(1, swSelectType_e.swSelSURFACEBODIES, swSelectType_e.swSelSOLIDBODIES)]
        [ParameterEditBody]
        [ControlOptions(height: 60)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
        [Description("Surface bodies to trim")]
        public List<IBody2> TargetBodies { get; set; }

        [SelectionBox(2, swSelectType_e.swSelSKETCHES)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectBoundary)]
        [ControlOptions(height: 60)]
        [ParameterSelection]
        [Description("Trim tools (either sketches or sketch regions)")]
        public List<object> TrimTools { get; set; }
    }
}
