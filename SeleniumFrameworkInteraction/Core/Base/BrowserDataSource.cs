using Core.Configuration;
using Core.DI;
using Core.Enum;

namespace Core.Base;

/// <summary>
/// Provides the list of browsers for <see cref="NUnit.Framework.TestFixtureSourceAttribute"/>.
/// Reads <see cref="DriverSettings.Browsers"/> from configuration.
/// Defaults to Chrome when the list is empty.
/// </summary>
public static class BrowserDataSource
{
    public static IEnumerable<BrowserType> Browsers
    {
        get
        {
            var browsers = ServiceLocator.GetService<IAppConfiguration>().DriverSettings.Browsers;
            return browsers?.Count > 0 ? browsers : new List<BrowserType> { BrowserType.Chrome };
        }
    }
}
