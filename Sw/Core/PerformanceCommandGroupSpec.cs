using CodeStack.Community.GeometryPlusPlus.Base;
using CodeStack.Community.GeometryPlusPlus.Properties;
using CodeStack.SwEx.AddIn.Core;

namespace CodeStack.Community.GeometryPlusPlus.Core
{
    public class PerformanceCommandGroupSpec : CommandGroupSpec
    {
        public PerformanceCommandGroupSpec(GeometryFeaturesCommandGroupSpec parent, PerformanceCommandSpec[] cmds)
        {
            Title = Resources.CommandGroupPerformanceTitle;
            Tooltip = Resources.CommandGroupPerformanceTooltip;
            Icon = new GeometryIcon(Resources.performance);
            Id = 1;
            Parent = parent;

            for (int i = 0; i < cmds.Length; i++)
            {
                cmds[i].SetUserId(i);
            }

            Commands = cmds;
        }
    }
}
