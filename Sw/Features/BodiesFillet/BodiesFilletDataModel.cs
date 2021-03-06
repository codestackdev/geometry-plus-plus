﻿//**********************
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

namespace CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet
{
    [Message("Select solid bodies, faces, edges to add fillets", "Add Fillet To Bodies")]
    [Help("https://www.codestack.net/labs/solidworks/geometry-plus-plus/user-guide/body-fillet/")]
    public class BodiesFilletPageDataModel
    {
        [SelectionBox(swSelectType_e.swSelSOLIDBODIES, swSelectType_e.swSelEDGES, swSelectType_e.swSelFACES, swSelectType_e.swSelVERTICES)]
        [ControlOptions(height: 60)]
        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_SelectFaceEdge)]
        [Description("Selections to add fillet to (solid bodies, faces or edges)")]
        public List<object> Selections { get; set; }

        [ControlAttribution(swControlBitmapLabelType_e.swBitmapLabel_Radius)]
        [NumberBoxOptions(swNumberboxUnitType_e.swNumberBox_Length, 0, 1000, 0.001,
                false, 0.01, 0.0005, swPropMgrPageNumberBoxStyle_e.swPropMgrPageNumberBoxStyle_Thumbwheel)]
        [Description("Fillet radius")]
        public double Radius { get; set; } = 0.01;
    }

    public class BodiesFilletFeatureDataModel
    {
        [ParameterSelection]
        public List<object> Selections { get; set; }

        [ParameterEditBody]
        public List<IBody2> EditBodies { get; set; }

        [ParameterDimension(swDimensionType_e.swRadialDimension)]
        public double Radius { get; set; } = 0.01;
    }
}
