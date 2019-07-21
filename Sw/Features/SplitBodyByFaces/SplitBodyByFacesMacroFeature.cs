//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using CodeStack.Community.GeometryPlusPlus.Exceptions;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.MacroFeature.Attributes;
using System.Runtime.InteropServices;
using SolidWorks.Interop.swconst;
using System.ComponentModel;

namespace CodeStack.Community.GeometryPlusPlus.Features.SplitBodyByFaces
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.split_body_by_faces))]
    [SwEx.Common.Attributes.Title("Split Body By Faces")]
    [Description("Converts solid or surface bodies faces to surface bodies")]
    [Options("SplitBodyByFaces", PROVIDER_MSG)]
    [SwEx.Common.Attributes.LoggerOptions(true, AddIn.LOGGER_NAME + ".SplitBodyByFacesMacroFeature")]
    [ComVisible(true), ProgId(PROG_ID), Guid("491EC32F-F879-4BFF-9C51-86AD93BA06E2")]
    public class SplitBodyByFacesMacroFeature : GeometryMacroFeature<SplitBodyByFacesDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.SplitBodyByFacesMacroFeature";

        protected override IBody2[] CreateGeometry(ISldWorks app, SplitBodyByFacesDataModel parameters)
        {
            if (parameters.Bodies != null && parameters.Bodies.Any())
            {
                var modeler = app.IGetModeler();

                var resBodies = new List<IBody2>();

                foreach (var body in parameters.Bodies)
                {
                    var faces = (body.GetFaces() as object[]).Cast<IFace2>().ToArray();

                    foreach (var face in faces)
                    {
                        var sheetBody = modeler.CreateSheetFromFaces(new IFace2[] { face });

                        resBodies.Add(sheetBody);
                    }
                }

                return resBodies.ToArray();
            }
            else
            {
                throw new UserErrorException("No input bodies selected");
            }
        }
    }
}
