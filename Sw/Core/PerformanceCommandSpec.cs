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
