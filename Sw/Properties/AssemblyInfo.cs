//**********************
//Geometry++ - Advanced geometry commands for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/geometry-plus-plus/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/geometry-plus-plus/
//**********************

using CodeStack.Community.GeometryPlusPlus.Properties;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xarial.AppLaunchKit.Attributes;
using Xarial.AppLaunchKit.Services.Attributes;

[assembly: AssemblyTitle("Geometry++")]
[assembly: AssemblyDescription("Advanced geometry commands for SOLIDWORKS")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("CodeStack")]
[assembly: AssemblyProduct("Geometry++")]
[assembly: AssemblyCopyright("Copyright © www.codestack.net 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("060b3bc3-0502-4903-96c8-35823ac63a50")]

[assembly: AssemblyVersion("0.4.0.0")]
[assembly: AssemblyFileVersion("0.4.0.0")]

[assembly: UpdatesUrl(typeof(Resources), nameof(Resources.UpdateUrl))]

[assembly: Log("CodeStack", "Geometry++", true, false)]
[assembly: About(typeof(Resources), nameof(Resources.eula), nameof(Resources.Licenses), nameof(Resources.logo))]

[assembly: ApplicationInfo(typeof(Resources), System.Environment.SpecialFolder.ApplicationData,
    nameof(Resources.WorkDir), nameof(Resources.AppTitle), nameof(Resources.icon))]
