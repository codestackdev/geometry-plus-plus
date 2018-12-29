using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.AppLaunchKit.Base.Services;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    internal class AboutCommand : Command
    {
        private readonly IAboutApplicationService m_AbtService;

        internal AboutCommand(IAboutApplicationService abtService, int id)
        {
            m_AbtService = abtService;

            UserId = id;
            Title = Resources.CommandAboutTitle;
            Tooltip = Resources.CommandAboutTooltip;
            HasMenu = true;
            HasToolbar = false;
            HasTabBox = false;
            SupportedWorkspace = swWorkspaceTypes_e.All;
            Icon = new MasterIcon(Resources.about_icon);
        }

        public override void OnClick()
        {
            m_AbtService.ShowAboutForm();
        }
    }
}
