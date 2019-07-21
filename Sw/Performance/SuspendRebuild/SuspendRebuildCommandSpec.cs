//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.Community.GeometryPlusPlus.Core;
using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn.Base;

namespace CodeStack.Community.GeometryPlusPlus.Performance.SuspendRebuild
{
    public class SuspendRebuildCommandSpec : PerformanceCommandSpec
    {
        private readonly ISldWorks m_App;
        private readonly IDocumentsHandler<SuspendRebuildDocumentHandler> m_DocsHandler;

        public SuspendRebuildCommandSpec(ISldWorks app, IDocumentsHandler<SuspendRebuildDocumentHandler> docsHandler)
        {
            Title = "Suspend Rebuild";
            Tooltip = "Suspends the rebuild operation";
            SupportedWorkspace = swWorkspaceTypes_e.AllDocuments;
            Icon = new GeometryIcon(Resources.suspend_rebuild);

            m_App = app;
            m_DocsHandler = docsHandler;
        }

        public override void OnClick()
        {
            var docHandler = m_DocsHandler[m_App.IActiveDoc2];

            docHandler.IsSuspended = !docHandler.IsSuspended;
        }

        public override CommandItemEnableState_e OnEnable()
        {
            try
            {
                //TODO: use isregistered when added to framework instead of exception
                var docHandler = m_DocsHandler[m_App.IActiveDoc2];

                if (docHandler.IsSuspended)
                {
                    return CommandItemEnableState_e.SelectEnable;
                }
                else
                {
                    return CommandItemEnableState_e.DeselectEnable;
                }
            }
            catch
            {
                return CommandItemEnableState_e.DeselectDisable;
            }
        }
    }
}
