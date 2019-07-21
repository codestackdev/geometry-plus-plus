//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.SwEx.Common.Attributes;
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

    [LoggerOptions(true, AddIn.LOGGER_NAME + ".PMPage")]
    internal class GeometryFeaturePropertyPage<TModel> : PropertyManagerPageEx<PropertyPageHandler, TModel>
    {
        internal GeometryFeaturePropertyPage(ISldWorks app, IPageSpec pageSpec) : base(app, pageSpec)
        {
        }
    }

    internal class PropertyPage<TPage>
    {
        internal event PageClosedDelegate<TPage> PageClosed;
        internal event PageApplying<TPage> PageApplying;

        internal event Action<ISldWorks, IModelDoc2, TPage> DataChanged;

        private readonly GeometryFeaturePropertyPage<TPage> m_Page;

        private readonly ISldWorks m_App;
        private IModelDoc2 m_Model;
        private TPage m_Data;
        private IFeature m_Feat;
        private IMacroFeatureData m_FeatData;

        internal PropertyPage(ISldWorks app, IPageSpec spec) 
        {
            m_App = app;

            m_Page = new GeometryFeaturePropertyPage<TPage> (m_App, spec);
            m_Page.Handler.DataChanged += OnDataChanged;
            m_Page.Handler.Closed += OnPageClosed;
            m_Page.Handler.Closing += OnClosing;
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(m_App, m_Model, m_Data);
        }

        internal void Show(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data)
        {
            m_Model = model;
            m_Data = data;

            m_Feat = feat;
            m_FeatData = featData;

            m_Page.Show(data);
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
