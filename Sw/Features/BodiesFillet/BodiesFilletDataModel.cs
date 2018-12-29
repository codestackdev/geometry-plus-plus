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

namespace CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet
{
    [Message("Select solid bodies, faces, edges to add fillets", "Add Fillet To Bodies")]
    public class BodiesFilletPageDataModel
    {
        [SelectionBox(swSelectType_e.swSelSOLIDBODIES, swSelectType_e.swSelEDGES, swSelectType_e.swSelFACES, swSelectType_e.swSelVERTICES)]
        [ControlOptions(height: 60)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceEdge)]
        [Description("Selections to add fillet to (solid bodies, faces or edges)")]
        public List<object> Selections { get; set; }

        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_Radius)]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.001,
                true, 0.01, 0.0005, swPropMgrPageNumberBoxStyle_e.swPropMgrPageNumberBoxStyle_Thumbwheel)]
        [Description("Fillet radius")]
        public double Radius { get; set; }
    }

    public class BodiesFilletFeatureDataModel
    {
        [ParameterSelection]
        public List<object> Selections { get; set; }

        [ParameterEditBody]
        public List<IBody2> EditBodies { get; set; }

        [ParameterDimension(swDimensionType_e.swRadialDimension)]
        public double Radius { get; set; }
    }
}
