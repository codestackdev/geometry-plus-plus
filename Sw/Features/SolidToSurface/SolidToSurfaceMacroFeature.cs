﻿//**********************
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

namespace CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface
{
    [SwEx.Common.Attributes.Icon(typeof(Resources), nameof(Resources.solid_to_surface))]
    [SwEx.Common.Attributes.Title("Convert Solid To Surface")]
    [Description("Converts solid bodies to surface bodies")]
    [Options("SolidToSurface", PROVIDER_MSG)]
    [SwEx.Common.Attributes.LoggerOptions(true, AddIn.LOGGER_NAME + ".SolidToSurfaceMacroFeature")]
    [ComVisible(true), ProgId(PROG_ID), Guid("753D2372-D1AF-4E22-94F2-6E6416394C9C")]
    public class SolidToSurfaceMacroFeature : GeometryMacroFeature<SolidToSurfaceDataModel>
    {
        internal const string PROG_ID = "CodeStack.GeometryPlusPlus.SolidToSurfaceMacroFeature";

        protected override IBody2[] CreateGeometry(ISldWorks app, SolidToSurfaceDataModel parameters)
        {
            if (parameters.Bodies != null && parameters.Bodies.Any())
            {
                var modeler = app.IGetModeler();

                var resBodies = new List<IBody2>();

                foreach (var body in parameters.Bodies)
                {
                    var faces = (body.GetFaces() as object[]).Cast<IFace2>().ToArray();

                    var sheetBody = modeler.CreateSheetFromFaces(faces);

                    resBodies.Add(sheetBody);
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
