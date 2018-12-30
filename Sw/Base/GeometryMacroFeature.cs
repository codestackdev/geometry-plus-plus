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
using CodeStack.SwEx.PMPage.Base;
using System.Drawing;
using CodeStack.SwEx.Common.Reflection;
using System.ComponentModel;
using CodeStack.SwEx.PMPage.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CodeStack.Community.GeometryPlusPlus.Base
{
    public interface IGeometryMacroFeature
    {
        void Insert(ISldWorks app, IModelDoc2 model);
    }

    public abstract class GeometryMacroFeature<TParams, TPage> : MacroFeatureEx<TParams>, IGeometryMacroFeature
        where TParams : class, new()
    {
        private readonly Color m_PreviewColor;

        private PageClosedDelegate<TPage> m_CurrentCloseHandler;
        private List<IBody2> m_CurrentEditBodies;
        private List<IBody2> m_CurrentPreviewBodies;
        private Exception m_LastError;
        private PropertyPage<TPage> m_Page;

        protected GeometryMacroFeature() 
            : this(Color.FromArgb(150, Color.Yellow))
        {
        }

        protected GeometryMacroFeature(Color previewColor)
        {
            m_PreviewColor = previewColor;
        }

        public void Insert(ISldWorks app, IModelDoc2 model)
        {
            ShowPropertyPage(app, model, null, null, new TParams(), OnFeatureInsertCompleted);
        }

        protected abstract TParams ConvertPageToParams(TPage page);

        protected abstract TPage ConvertParamsToPage(TParams parameters);

        protected abstract IBody2[] CreateGeometry(ISldWorks app, TParams parameters);

        protected virtual IBody2[] CreatePreview(ISldWorks app, TParams parameters, ref IBody2[] editBodies)
        {
            return CreateGeometry(app, parameters);
        }

        protected override bool OnEditDefinition(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            var featData = feature.GetDefinition() as IMacroFeatureData;
            featData.AccessSelections(model, null);

            var parameters = GetParameters(feature, featData, model);

            ShowPropertyPage(app, model, feature, featData, parameters, OnFeatureEditCompleted);

            return true;
        }

        protected override MacroFeatureRebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature, TParams parameters)
        {
            try
            {
                var featData = feature.GetDefinition() as IMacroFeatureData;

                return CreateRebuildResult(app, featData, parameters);
            }
            catch (UserErrorException ex)
            {
                Logger.Log(ex);
                return MacroFeatureRebuildResult.FromStatus(false, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return MacroFeatureRebuildResult.FromStatus(false, "Rebuild error");
            }
        }

        protected virtual MacroFeatureRebuildResult CreateRebuildResult(ISldWorks app, IMacroFeatureData featData, TParams parameters)
        {
            var bodies = CreateGeometry(app, parameters);
            
            if (bodies.Any())
            {
                return MacroFeatureRebuildResult.FromBodies(bodies, featData, true);
            }
            else
            {
                return MacroFeatureRebuildResult.FromStatus(true);
            }
        }

        private PropertyPage<TPage> GetPage(ISldWorks app)
        {
            if (m_Page == null)
            {
                m_Page = new PropertyPage<TPage>(app, GetPageSpec());
                m_Page.DataChanged += OnDataChanged;
                m_Page.PageClosed += OnPageClosed;
                m_Page.PageApplying += OnPageApplying;
            }

            return m_Page;
        }

        private IPageSpec GetPageSpec()
        {
            Image icon = null;
            string title = "";

            if (!this.GetType().TryGetAttribute<SwEx.Common.Attributes.IconAttribute>(a => icon = a.Icon))
            {
                throw new NullReferenceException("Icon attribute not set");
            }

            if (!this.GetType().TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName))
            {
                throw new NullReferenceException("Icon attribute not set");
            }

            return new PageSpec(title, icon, swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton
                | swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton);
        }
        private void HidePreview(IModelDoc2 model, IBody2[] editBodiesToShow)
        {
            try
            {
                if (editBodiesToShow != null)
                {
                    foreach (var editBody in editBodiesToShow)
                    {
                        if (editBody != null)
                        {
                            editBody.DisableDisplay = false;
                            editBody.DisableHighlight = false;
                        }

                        m_CurrentEditBodies.Remove(editBody);
                    }
                }

                if (m_CurrentPreviewBodies != null)
                {
                    for (int i = 0; i < m_CurrentPreviewBodies.Count; i++)
                    {
                        if (m_CurrentPreviewBodies[i] != null)
                        {
                            m_CurrentPreviewBodies[i].Hide(model);
                            Marshal.ReleaseComObject(m_CurrentPreviewBodies[i]);
                            m_CurrentPreviewBodies[i] = null;
                        }
                    }

                    m_CurrentPreviewBodies.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            model.GraphicsRedraw2();
        }

        private void OnDataChanged(ISldWorks app, IModelDoc2 model, TPage data)
        {
            var parameters = ConvertPageToParams(data);
            PregenerateAndPreviewGeometry(app, model, parameters);
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

        private void OnFeatureInsertCompleted(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk)
        {
            Logger.Log("Inserting new feature");

            HidePreview(model, m_CurrentEditBodies.ToArray());

            if (isOk)
            {
                try
                {
                    var parameters = ConvertPageToParams(data);

                    var newFeat = model.FeatureManager.InsertComFeature(GetType(), parameters);
                }
                catch(Exception ex)
                {
                    Logger.Log(ex);
                    throw;
                }
            }
        }

        private void OnPageApplying(ISldWorks app, IModelDoc2 model, TPage data, SwEx.PMPage.Base.ClosingArg arg)
        {
            if (m_LastError != null)
            {
                Logger.Log(m_LastError);

                arg.Cancel = true;
                arg.ErrorTitle = Resources.MacroFeatureErrorInvalidParameters;

                if (m_LastError is UserErrorException)
                {
                    arg.ErrorMessage = m_LastError.Message;
                }
                else
                {
                    arg.ErrorMessage = "Unknown error";
                }
            }
        }

        private void OnPageClosed(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TPage data, bool isOk)
        {
            m_CurrentCloseHandler?.Invoke(model, feat, featData, data, isOk);
        }

        private void PregenerateAndPreviewGeometry(ISldWorks app, IModelDoc2 model, TParams parameters)
        {
            m_LastError = null;

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

            IBody2[] geom = null;

            try
            {
                geom = CreatePreview(app, parameters, ref editBodies);
            }
            catch (Exception ex)
            {
                editBodies = null;
                m_LastError = ex;
            }
            finally
            {
                ResetPreview(model, editBodies);

                PreviewNewGeometry(model, geom, m_PreviewColor);

                model.GraphicsRedraw2();
            }
        }

        private void PreviewNewGeometry(IModelDoc2 model, IBody2[] geom, Color color)
        {
            if (geom != null)
            {
                foreach (var body in geom)
                {
                    var colorRef = ConvertColor(color);

                    body.Display3(model, colorRef,
                        (int)swTempBodySelectOptions_e.swTempBodySelectOptionNone);
                    
                    body.MaterialPropertyValues2 = new double[] 
                    {
                        color.R / 255d, color.G / 255d, color.B / 255d,
                        0.5, 0.5, 0.5, 0.5,
                        (255 - color.A) / 255d, 0.5
                    };
                }

                m_CurrentPreviewBodies.AddRange(geom);
            }
        }

        private void ResetPreview(IModelDoc2 model, IBody2[] editBodies)
        {
            if (editBodies == null)
            {
                editBodies = new IBody2[0];
            }

            HidePreview(model, m_CurrentEditBodies.Except(editBodies).ToArray());

            var newEditBodies = editBodies.Except(m_CurrentEditBodies).ToArray();

            m_CurrentEditBodies.AddRange(newEditBodies);

            foreach (var editBody in newEditBodies)
            {
                if (editBody != null)
                {
                    editBody.DisableDisplay = true;
                    editBody.DisableHighlight = true;
                }
            }
        }

        private void ShowPropertyPage(ISldWorks app, IModelDoc2 model, IFeature feat,
            IMacroFeatureData featData, TParams parameters, PageClosedDelegate<TPage> closeHandler)
        {
            m_CurrentEditBodies = new List<IBody2>();
            m_CurrentPreviewBodies = new List<IBody2>();
            m_LastError = null;

            m_CurrentCloseHandler = closeHandler;

            var data = ConvertParamsToPage(parameters);

            GetPage(app).Show(model, feat, featData, data);

            PregenerateAndPreviewGeometry(app, model, parameters);
        }

        protected int ConvertColor(Color color)
        {
            return (color.R << 0) | (color.G << 8) | (color.B << 16);
        }
    }

    public abstract class GeometryMacroFeature<TParams> : GeometryMacroFeature<TParams, TParams>
        where TParams : class, new()
    {
        protected GeometryMacroFeature() : base()
        {
        }

        protected GeometryMacroFeature(Color previewColor) : base(previewColor)
        {
        }

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
