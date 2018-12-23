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

namespace CodeStack.Community.GeometryPlusPlus.Features.TrimSurfacesByRegion
{
    [PageOptions(typeof(Resources), nameof(Resources.solid_to_surface),
        swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton)]
    [Message("Select solid bodies to convert to surface bodies", "Convert Solid To Surface")]
    [DisplayName("Convert Solid To Surface")]
    public class TrimSurfacesByRegionDataModel
    {
        [SelectionBox(1, swSelectType_e.swSelSURFACEBODIES)]
        [ParameterEditBody]
        [ControlOptions(height: 60)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
        public List<IBody2> TargetBodies { get; set; }

        [SelectionBox(2, swSelectType_e.swSelSKETCHES, swSelectType_e.swSelSKETCHREGION)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectBoundary)]
        [ControlOptions(height: 60)]
        [ParameterSelection]
        public List<object> TrimTools { get; set; }
    }
}
