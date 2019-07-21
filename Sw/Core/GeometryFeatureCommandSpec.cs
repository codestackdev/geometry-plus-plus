//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    internal class GeometryFeatureCommandSpec : CommandSpec
    {
        private readonly IGeometryMacroFeature m_Feat;
        private readonly ISldWorks m_App;

        internal GeometryFeatureCommandSpec(ISldWorks app, IGeometryMacroFeature feat, int id)
        {
            if (feat == null)
            {
                throw new ArgumentNullException(nameof(feat));
            }

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            m_Feat = feat;
            m_App = app;

            var type = feat.GetType();

            if (!type.TryGetAttribute<DisplayNameAttribute>(a => Title = a.DisplayName))
            {
                throw new NullReferenceException($"{type.FullName} must be decorated with {typeof(DisplayNameAttribute).FullName}");
            }

            if (!type.TryGetAttribute<DescriptionAttribute>(a => Tooltip = a.Description))
            {
                throw new NullReferenceException($"{type.FullName} must be decorated with {typeof(DescriptionAttribute).FullName}");
            }

            if (!type.TryGetAttribute<IconAttribute>(a => Icon = new GeometryIcon(a.Icon)))
            {
                throw new NullReferenceException($"{type.FullName} must be decorated with {typeof(IconAttribute).FullName}");
            }

            UserId = id;
            HasMenu = true;
            HasToolbar = true;
            HasTabBox = true;
            TabBoxStyle = SolidWorks.Interop.swconst.swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;
            SupportedWorkspace = swWorkspaceTypes_e.Part;
        }

        public override void OnClick()
        {
            m_Feat.Insert(m_App, m_App.IActiveDoc2);
        }
    }
}
