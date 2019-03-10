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

namespace CodeStack.Community.GeometryPlusPlus.Features.SplitBodyByFaces
{
    [Message("Select solid or surface bodies to convert faces to surface bodies", "Split Body By Faces")]
    [Help("https://www.codestack.net/labs/solidworks/geometry-plus-plus/user-guide/split-body-by-faces/")]
    public class SplitBodyByFacesDataModel
    {
        [SelectionBox(swSelectType_e.swSelSOLIDBODIES, swSelectType_e.swSelSURFACEBODIES)]
        [ParameterEditBody]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceSurface)]
        [ControlOptions(height: 60)]
        [Description("Solid or sheet bodies to convert faces to surface bodies")]
        public List<IBody2> Bodies { get; set; }
    }
}
