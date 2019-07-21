//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    public class PerformanceCommandSpec : CommandSpec
    {
        public PerformanceCommandSpec()
        {
            HasMenu = true;
            HasToolbar = true;
            HasTabBox = true;
            TabBoxStyle = SolidWorks.Interop.swconst.swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;
        }

        internal void SetUserId(int userId)
        {
            UserId = userId;
        }
    }
}
