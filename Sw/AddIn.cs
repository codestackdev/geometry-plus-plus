using CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet;
using CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap;
using CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface;
using CodeStack.Community.GeometryPlusPlus.Features.TrimSurfacesByRegion;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
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
    public class AddIn : SwAddInEx
    {
        //TODO: redundancy - make commands to load from the available macro features

        [Title("Geometry++")]
        [Icon(typeof(Resources), nameof(Resources.geometry_plus_plus))]
        private enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.solid_to_surface))]
            [Title("Convert Solid To Surface")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            SolidToSurface,

            [Icon(typeof(Resources), nameof(Resources.trim_surface_region))]
            [Title("Trim Surface By Region")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            TrimSurfaceByRegion,

            [Icon(typeof(Resources), nameof(Resources.extrude_surface_caps))]
            [Title("Extrude Surface Cap")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            ExtrudeSurfaceCap,

            [Icon(typeof(Resources), nameof(Resources.fillet))]
            [Title("Bodies Fillet")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            BodiesFillet,

            [Title("About...")]
            [Description("About Geometry++")]
            [CommandItemInfo(true, false, swWorkspaceTypes_e.All)]
            [Icon(typeof(Resources), nameof(Resources.about_icon))]
            About
        }

        private ServicesManager m_Kit;

        public override bool OnConnect()
        {   
            m_Kit = new ServicesManager(this.GetType().Assembly, new IntPtr(App.IFrameObject().GetHWnd()),
                typeof(UpdatesService),
                typeof(SystemEventLogService),
                typeof(AboutApplicationService));

            m_Kit.HandleError += OnHandleError;

            var syncContext = SynchronizationContext.Current;

            if (syncContext == null)
            {
                syncContext = new System.Windows.Forms.WindowsFormsSynchronizationContext();
            }

            Task.Run(() =>
            {
                SynchronizationContext.SetSynchronizationContext(
                        syncContext);
                m_Kit.StartServicesAsync().Wait();
            });

            this.AddCommandGroup<Commands_e>(OnButtonClicked);

            return true;
        }

        private bool OnHandleError(Exception ex)
        {
            try
            {
                m_Kit.GetService<ILogService>().LogException(ex);
            }
            catch
            {
            }

            return true;
        }

        private void OnButtonClicked(Commands_e btn)
        {
            switch (btn)
            {
                //TODO: use dependency injection with singleton

                case Commands_e.SolidToSurface:
                    {
                        new SolidToSurfaceMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                case Commands_e.TrimSurfaceByRegion:
                    {
                        new TrimSurfacesByRegionMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                case Commands_e.ExtrudeSurfaceCap:
                    {
                        App.SendMsgToUser("Not implemented");
                        new ExtrudeSurfaceCapMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                case Commands_e.BodiesFillet:
                    {
                        App.SendMsgToUser("Not implemented");
                        new BodiesFilletMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                case Commands_e.About:
                    m_Kit.GetService<IAboutApplicationService>().ShowAboutForm();
                    break;

                default:
                    App.SendMsgToUser("Not implemented");
                    break;
            }
        }
    }
}
