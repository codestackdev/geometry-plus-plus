using CodeStack.Community.GeometryPlusPlus.Core;
using CodeStack.Community.GeometryPlusPlus.Features.BodiesFillet;
using CodeStack.Community.GeometryPlusPlus.Features.ExtrudeSurfaceCap;
using CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface;
using CodeStack.Community.GeometryPlusPlus.Performance.SuspendRebuild;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.Common.Attributes;
using System;
using System.Runtime.InteropServices;

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
            var docsHandler = CreateDocumentsHandler<SuspendRebuildDocumentHandler>();
            
            m_Services = new ServicesContainer(App, docsHandler);
            
            var cmdBar = m_Services.GetService<GeometryFeaturesCommandGroupSpec>();
            var perfCmdBar = m_Services.GetService<PerformanceCommandGroupSpec>();

            this.AddCommandGroup(cmdBar);
            this.AddCommandGroup(perfCmdBar);

            return true;
        }
    }
}
