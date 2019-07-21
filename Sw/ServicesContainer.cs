//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.Community.GeometryPlusPlus.Core;
using CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet;
using CodeStack.Community.GeometryPlusPlus.Features.CropBodies;
using CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap;
using CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface;
using CodeStack.Community.GeometryPlusPlus.Features.SplitBodyByFaces;
using CodeStack.Community.GeometryPlusPlus.Performance.SuspendRebuild;
using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;
using Xarial.AppLaunchKit;
using Xarial.AppLaunchKit.Base.Services;
using Xarial.AppLaunchKit.Services.About;
using Xarial.AppLaunchKit.Services.Logger;
using Xarial.AppLaunchKit.Services.Updates;

namespace CodeStack.Community.GeometryPlusPlus
{
    public class ServicesContainer
    {
        public static ServicesContainer Instance
        {
            get;
            private set;
        }

        private readonly UnityContainer m_Container;
        private readonly ServicesManager m_Kit;

        public ServicesContainer(ISldWorks app, IDocumentsHandler<SuspendRebuildDocumentHandler> docsHandler)
        {
            Instance = this;

            m_Container = new UnityContainer();

            m_Container.RegisterInstance(app);

            m_Kit = RegisterServicesManager(app);

            m_Container.RegisterInstance(docsHandler);

            foreach (var geomFeat in RegisterGeometryFeatures())
            {
                m_Container.RegisterInstance(geomFeat.GetType().Name, geomFeat);
            }

            foreach (var perfCmdType in RegisterPerformanceCommands())
            {
                m_Container.RegisterType(typeof(PerformanceCommandSpec), perfCmdType, perfCmdType.Name);
            }

            m_Container.RegisterType<GeometryFeaturesCommandGroupSpec>(new ContainerControlledLifetimeManager());
            m_Container.RegisterType<PerformanceCommandGroupSpec>(new ContainerControlledLifetimeManager());
            
            m_Container.RegisterInstance(m_Kit.GetService<ILogService>());
            m_Container.RegisterInstance(m_Kit.GetService<IAboutApplicationService>());
        }

        private ServicesManager RegisterServicesManager(ISldWorks app)
        {
            var kit = new ServicesManager(this.GetType().Assembly, new IntPtr(app.IFrameObject().GetHWnd()),
                typeof(UpdatesService),
                typeof(SystemEventLogService),
                typeof(AboutApplicationService));

            kit.HandleError += OnHandleError;

            var syncContext = SynchronizationContext.Current;

            if (syncContext == null)
            {
                syncContext = new System.Windows.Forms.WindowsFormsSynchronizationContext();
            }

            Task.Run(() =>
            {
                SynchronizationContext.SetSynchronizationContext(
                        syncContext);
                kit.StartServicesAsync().Wait();
            });

            return kit;
        }

        private IEnumerable<IGeometryMacroFeature> RegisterGeometryFeatures()
        {
            yield return new SolidToSurfaceMacroFeature();
            yield return new CropBodiesMacroFeature();
            yield return new ExtrudeSurfaceCapMacroFeature();
            yield return new BodiesFilletMacroFeature();
            yield return new SplitBodyByFacesMacroFeature();
        }

        private IEnumerable<Type> RegisterPerformanceCommands()
        {
            yield return typeof(SuspendRebuildCommandSpec);
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

        internal TService GetService<TService>()
        {
            return m_Container.Resolve<TService>();
        }
    }
}
