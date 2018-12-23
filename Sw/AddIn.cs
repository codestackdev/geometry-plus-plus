using CodeStack.Community.GeometryPlusPlus.Features.SolidToSurface;
using CodeStack.Community.GeometryPlusPlus.Features.TrimSurfacesByRegion;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus
{
    [ComVisible(true)]
    [Guid("45B95E54-91F5-4043-BC30-20FD89CAB578")]
    [AutoRegister("Geometry++", "Additional geometry functionality for SOLIDWORKS")]
    public class AddIn : SwAddInEx
    {
        //TODO: redundancy - make commands to load from the available macro features

        [Title("Geometry++")]
        [Icon(typeof(Resources), nameof(Resources.geometry_plus_plus))]
        private enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.solid_to_surface))]
            [Title("Convert Solid To Surface")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            SolidToSurface,

            [Icon(typeof(Resources), nameof(Resources.trim_surface_region))]
            [Title("Trim Surface By Region")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            TrimSurfaceByRegion,

            [Icon(typeof(Resources), nameof(Resources.extrude_surface_caps))]
            [Title("Extrude Surface Cap")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            ExtrudeSurfaceCap,

            [Icon(typeof(Resources), nameof(Resources.fillet))]
            [Title("Bodies Fillet")]
            [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
            BodiesFillet
        }

        public override bool OnConnect()
        {
            this.AddCommandGroup<Commands_e>(OnButtonClicked);
            return true;
        }

        private void OnButtonClicked(Commands_e btn)
        {
            switch (btn)
            {
                //TODO: use dependency injection with singleton

                case Commands_e.SolidToSurface:
                    {
                        new SolidToSurfaceMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                case Commands_e.TrimSurfaceByRegion:
                    {
                        new TrimSurfacesByRegionMacroFeature().Insert(App, App.IActiveDoc2);
                        break;
                    }

                default:
                    App.SendMsgToUser("Not implemented");
                    break;
            }
        }
    }
}
