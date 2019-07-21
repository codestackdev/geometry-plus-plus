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

namespace CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface
{
    [Message("Select solid bodies to convert to surface bodies", "Convert Solid To Surface")]
    [Help("https://www.codestack.net/labs/solidworks/geometry-plus-plus/user-guide/convert-solid-to-surface/")]
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
