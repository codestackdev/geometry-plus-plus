using CodeStack.SwEx.AddIn.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace CodeStack.Community.GeometryPlusPlus.Performance.SuspendRebuild
{
    public class SuspendRebuildDocumentHandler : IDocumentHandler
    {
        public bool IsSuspended { get; set; }

        private ISldWorks m_App;
        private IModelDoc2 m_Model;
        private int m_SuspendedRebuildsCount;

        public void Init(ISldWorks app, IModelDoc2 model)
        {
            m_App = app;
            m_Model = model;
            m_SuspendedRebuildsCount = 0;

            if (m_Model is PartDoc)
            {
                (m_Model as PartDoc).RegenNotify += OnRegeneration;
            }
            else if (m_Model is AssemblyDoc)
            {
                (m_Model as AssemblyDoc).RegenNotify += OnRegeneration;
            }
            else if (m_Model is DrawingDoc)
            {
                (m_Model as DrawingDoc).RegenNotify += OnRegeneration;
            }
        }

        public void Dispose()
        {
            if (m_Model is PartDoc)
            {
                (m_Model as PartDoc).RegenNotify -= OnRegeneration;
            }
            else if (m_Model is AssemblyDoc)
            {
                (m_Model as AssemblyDoc).RegenNotify -= OnRegeneration;
            }
            else if (m_Model is DrawingDoc)
            {
                (m_Model as DrawingDoc).RegenNotify -= OnRegeneration;
            }
        }

        private int OnRegeneration()
        {
            const int S_OK = 0;
            const int S_CANCEL = 1;

            if (IsSuspended)
            {
                m_SuspendedRebuildsCount++;
                m_App.IFrameObject().SetStatusBarText(
                    $"Suspended {m_SuspendedRebuildsCount} rebuild{(m_SuspendedRebuildsCount > 1 ? "s" : "")}");
                return S_CANCEL;
            }
            else
            {
                m_SuspendedRebuildsCount = 0;
                return S_OK;
            }
        }
    }
}
