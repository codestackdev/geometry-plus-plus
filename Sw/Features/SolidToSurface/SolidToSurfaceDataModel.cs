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

namespace CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface
{
    [Message("Select solid bodies to convert to surface bodies", "Convert Solid To Surface")]
    public class SolidToSurfaceDataModel
    {
        [SelectionBox(swSelectType_e.swSelSOLIDBODIES)]
        [ParameterEditBody]
        [ControlAttribution(typeof(Resources), nameof(Resources.solid_body))]
        [ControlOptions(height: 60)]
        [Description("Solid bodies to convert to surface")]
        public List<IBody2> Bodies { get; set; }
    }
}
