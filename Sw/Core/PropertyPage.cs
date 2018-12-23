using CodeStack.SwEx.PMPage;
using CodeStack.SwEx.PMPage.Base;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    [ComVisible(true)]
    [Guid("B8A7D8B3-FC3C-4833-86D2-D36E35F63B8C")]
    public class PropertyPageHandler : PropertyManagerPageHandlerEx
    {
    }

    internal delegate void PageClosedDelegate<TPage>(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk);
    internal delegate void PageApplying<TPage>(ISldWorks app, IModelDoc2 model, TPage data, ClosingArg arg);

    internal class PropertyPage<TPage>
    {
        internal event PageClosedDelegate<TPage> PageClosed;
        internal event PageApplying<TPage> PageApplying;

        internal event Action<ISldWorks, IModelDoc2, TPage> DataChanged;

        private readonly PropertyManagerPageEx<PropertyPageHandler, TPage> m_Page;

        private readonly ISldWorks m_App;
        private readonly IModelDoc2 m_Model;
        private readonly TPage m_Data;
        private readonly IFeature m_Feat;
        private IMacroFeatureData m_FeatData;

        internal PropertyPage(ISldWorks app, IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data) 
        {
            m_App = app;
            m_Model = model;
            m_Data = data;

            m_Feat = feat;
            m_FeatData = featData;

            m_Page = new PropertyManagerPageEx<PropertyPageHandler, TPage>(m_Data, m_App);
            m_Page.Handler.DataChanged += OnDataChanged;
            m_Page.Handler.Closed += OnPageClosed;
            m_Page.Handler.Closing += OnClosing;
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(m_App, m_Model, m_Data);
        }

        internal void Show()
        {
            m_Page.Show();
        }

        private void OnPageClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            PageClosed?.Invoke(m_Model, m_Feat, m_FeatData, m_Data,
                reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay);
        }

        private void OnClosing(swPropertyManagerPageCloseReasons_e reason, ClosingArg arg)
        {
            if (reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                PageApplying?.Invoke(m_App, m_Model, m_Data, arg);
            }
        }
    }
}
