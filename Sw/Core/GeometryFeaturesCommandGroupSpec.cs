//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using System.Collections.Generic;
using Xarial.AppLaunchKit.Base.Services;
using SolidWorks.Interop.sldworks;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    public class GeometryFeaturesCommandGroupSpec : CommandGroupSpec
    {
        public GeometryFeaturesCommandGroupSpec(ISldWorks app, IGeometryMacroFeature[] features, 
            IAboutApplicationService abtService)
        {
            Title = Resources.CommandBarGeometryTitle;
            Tooltip = Resources.CommandBarGeometryTooltip;
            Icon = new GeometryIcon(Resources.geometry_plus_plus);
            Id = 0;
            Commands = GetCommands(features, abtService, app);
        }

        private ICommandSpec[] GetCommands(IGeometryMacroFeature[] features, 
            IAboutApplicationService abtService, ISldWorks app)
        {
            var cmds = new List<ICommandSpec>();

            for (int i = 0; i < features.Length; i++)
            {
                cmds.Add(new GeometryFeatureCommandSpec(app, features[i], i));
            }
            
            cmds.Add(new AboutCommandSpec(abtService, cmds.Count));

            return cmds.ToArray();
        }
    }
}
