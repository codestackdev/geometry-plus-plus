using CodeStack.Community.GeometryPlusPlus.Core;
using CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet;
using CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap;
using CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.AppLaunchKit;
using Xarial.AppLaunchKit.Base.Services;
using Xarial.AppLaunchKit.Services.About;
using Xarial.AppLaunchKit.Services.Logger;
using Xarial.AppLaunchKit.Services.Updates;

namespace CodeStack.Community.GeometryPlusPlus
{
    [ComVisible(true)]
    [Guid("45B95E54-91F5-4043-BC30-20FD89CAB578")]
#if DEBUG
    [AutoRegister("Geometry++", "Additional geometry functionality for SOLIDWORKS")]
#endif
    [LoggerOptions(true, LOGGER_NAME)]
    public class AddIn : SwAddInEx
    {
        internal const string LOGGER_NAME = "Geometry++";

        private ServicesContainer m_Services;

        public override bool OnConnect()
        {
            m_Services = new ServicesContainer(App);

            var cmdBar = m_Services.GetService<GeometryFeaturesCommandGroupSpec>();

            this.AddCommandGroup(cmdBar);

            return true;
        }
    }
}
