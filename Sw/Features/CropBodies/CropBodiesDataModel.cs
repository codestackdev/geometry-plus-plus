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
    [PageOptions(typeof(Resources), nameof(Resources.trim_surface_region),
        swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton)]
    [Message("Trims selected surface bodies with selected region", "Trim Surfaces By Region")]
    [DisplayName("Trim Surfaces By Region")]
    public class CropBodiesDataModel
    {
        [SelectionBox(1, swSelectType_e.swSelSURFACEBODIES, swSelectType_e.swSelSOLIDBODIES)]
        [ParameterEditBody]
        [ControlOptions(height: 60)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
        [Description("Surface bodies to trim")]
        public List<IBody2> TargetBodies { get; set; }

        [SelectionBox(2, swSelectType_e.swSelSKETCHES, swSelectType_e.swSelSKETCHREGION)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectBoundary)]
        [ControlOptions(height: 60)]
        [ParameterSelection]
        [Description("Trim tools (either sketches or sketch regions)")]
        public List<object> TrimTools { get; set; }
    }
}
