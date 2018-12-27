using CodeStack.SwEx.MacroFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.Community.GeometryPlusPlus.Core;
using CodeStack.Community.GeometryPlusPlus.Exceptions;
using CodeStack.Community.GeometryPlusPlus.Properties;
using SolidWorks.Interop.swconst;

namespace CodeStack.Community.GeometryPlusPlus.Base
{
    //TODO: macro feature icon and page model - is a redundancy - need to only specify it once
    public abstract class GeometryMacroFeature<TParams, TPage> : MacroFeatureEx<TParams>
        where TParams : class, new()
    {
        private const int PREVIEW_COLORREF = 65535; //yellow

        private PropertyPage<TPage> m_Page;

        private List<IBody2> m_CurrentEditBodies;
        private List<IBody2> m_CurrentPreviewBodies;
        private PageClosedDelegate<TPage> m_CurrentCloseHandler;

        private PropertyPage<TPage> GetPage(ISldWorks app)
        {
            if (m_Page == null)
            {
                m_Page = new PropertyPage<TPage>(app);
                m_Page.DataChanged += OnDataChanged;
                m_Page.PageClosed += OnPageClosed;
                m_Page.PageApplying += OnPageApplying;
            }

            return m_Page;
        }
        
        internal void Insert(ISldWorks app, IModelDoc2 model)
        {
            ShowPropertyPage(app, model, null, null, new TParams(), OnFeatureInsertCompleted);
        }

        private void OnFeatureInsertCompleted(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk)
        {
            HidePreview(model, m_CurrentEditBodies.ToArray());

            if (isOk)
            {
                var parameters = ConvertPageToParams(data);

                var newFeat = model.FeatureManager.InsertComFeature(GetType(), parameters);
            }
        }

        private void PreviewGeometry(ISldWorks app, IModelDoc2 model, TParams parameters)
        {
            string[] paramNames;
            int[] paramTypes;
            string[] paramValues;
            object[] selection;
            int[] dimTypes;
            double[] dimValues;
            IBody2[] editBodies;

            ParseParameters(parameters,
                out paramNames, out paramTypes, out paramValues, out selection,
                out dimTypes, out dimValues, out editBodies);

            if (editBodies == null)
            {
                editBodies = new IBody2[0];
            }

            HidePreview(model, m_CurrentEditBodies.Except(editBodies).ToArray());

            if (editBodies != null)
            {
                var newEditBodies = editBodies.Except(m_CurrentEditBodies).ToArray();

                m_CurrentEditBodies.AddRange(newEditBodies);

                foreach (var editBody in newEditBodies)
                {
                    editBody.DisableDisplay = true;
                    editBody.DisableHighlight = true;
                }
            }

            try
            {
                var geom = CreateGeometry(app, parameters);

                if (geom != null)
                {
                    foreach (var body in geom)
                    {
                        body.Display3(model, PREVIEW_COLORREF,
                            (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);

                        body.MaterialPropertyValues2 = new double[] { 1, 1, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
                    }

                    m_CurrentPreviewBodies.AddRange(geom);
                }

                model.GraphicsRedraw2();
            }
            catch
            {
            }
        }

        private void HidePreview(IModelDoc2 model, IBody2[] editBodiesToShow)
        {
            if (editBodiesToShow != null)
            {
                foreach (var editBody in editBodiesToShow)
                {
                    editBody.DisableDisplay = false;
                    editBody.DisableHighlight = false;
                    m_CurrentEditBodies.Remove(editBody);
                }
            }

            if (m_CurrentPreviewBodies != null)
            {
                for (int i = 0; i < m_CurrentPreviewBodies.Count; i++)
                {
                    m_CurrentPreviewBodies[i].Hide(model);
                    m_CurrentPreviewBodies[i] = null;
                }

                m_CurrentPreviewBodies.Clear();
            }

            GC.Collect();
            model.GraphicsRedraw2();
        }

        protected override bool OnEditDefinition(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            var featData = feature.GetDefinition() as IMacroFeatureData;
            featData.AccessSelections(model, null);

            var parameters = GetParameters(feature, featData, model);

            ShowPropertyPage(app, model, feature, featData, parameters, OnFeatureEditCompleted);
            
            return true;
        }
        
        private void ShowPropertyPage(ISldWorks app, IModelDoc2 model, IFeature feat,
            IMacroFeatureData featData, TParams parameters, PageClosedDelegate<TPage> closeHandler)
        {
            m_CurrentEditBodies = new List<IBody2>();
            m_CurrentPreviewBodies = new List<IBody2>();
            m_CurrentCloseHandler = closeHandler;

            var data = ConvertParamsToPage(parameters);
            
            GetPage(app).Show(model, feat, featData, data);

            PreviewGeometry(app, model, parameters);
        }

        private void OnPageApplying(ISldWorks app, IModelDoc2 model, TPage data, SwEx.PMPage.Base.ClosingArg arg)
        {
            try
            {
                var parameters = ConvertPageToParams(data);
                var geom = CreateGeometry(app, parameters);
            }
            catch(UserErrorException ex)
            {
                arg.Cancel = true;
                arg.ErrorMessage = ex.Message;
                arg.ErrorTitle = Resources.MacroFeatureErrorInvalidParameters;
            }
            catch
            {
                arg.Cancel = true;
                arg.ErrorMessage = "Unknown error";
                arg.ErrorTitle = Resources.MacroFeatureErrorInvalidParameters;
            }
        }

        private void OnDataChanged(ISldWorks app, IModelDoc2 model, TPage data)
        {
            var parameters = ConvertPageToParams(data);
            PreviewGeometry(app, model, parameters);
        }

        private void OnPageClosed(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk)
        {
            m_CurrentCloseHandler?.Invoke(model, feat, featData, data, isOk);
        }

        private void OnFeatureEditCompleted(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk)
        {
            HidePreview(model, m_CurrentEditBodies.ToArray());

            if (isOk)
            {
                var parameters = ConvertPageToParams(data);
                SetParameters(model, feat, featData, parameters);
                feat.ModifyDefinition(featData, model, null);
            }
            else
            {
                featData.ReleaseSelectionAccess();
            }
        }

        protected abstract TPage ConvertParamsToPage(TParams parameters);
        protected abstract TParams ConvertPageToParams(TPage page);

        protected override MacroFeatureRebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature, TParams parameters)
        {
            var bodies = CreateGeometry(app, parameters);

            var featData = feature.GetDefinition() as IMacroFeatureData;

            return MacroFeatureRebuildResult.FromBodies(bodies, featData, true);
        }

        protected abstract IBody2[] CreateGeometry(ISldWorks app, TParams parameters);
    }

    public abstract class GeometryMacroFeature<TParams> : GeometryMacroFeature<TParams, TParams>
        where TParams : class, new()
    {
        protected sealed override TParams ConvertPageToParams(TParams page)
        {
            return page;
        }

        protected sealed override TParams ConvertParamsToPage(TParams parameters)
        {
            return parameters;
        }
    }
}
