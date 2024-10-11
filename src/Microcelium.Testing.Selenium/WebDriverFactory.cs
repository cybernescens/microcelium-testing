using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium;

/// <summary>
///   Builds the WebDriver
/// </summary>
public class WebDriverFactory
{
  private readonly WebDriverConfig configuration;
  private readonly AssemblyScanner scanner;

  public WebDriverFactory(WebDriverConfig configuration)
  {
    this.configuration = configuration;
    scanner = new AssemblyScanner { ThrowExceptions = false };
  }

  /// <summary>
  ///   Creates a web driver
  /// </summary>
  public IWebDriver Create(RuntimeConfig runtime)
  {
    var results = scanner.GetScannableAssemblies();
    var type = results.Types.FirstOrDefault(
      x => x.FullName!.Equals(configuration.Browser.DriverFactory, StringComparison.OrdinalIgnoreCase));

    if (type == null)
      throw new InvalidOperationException($"No type found for `{configuration.Browser.DriverFactory}");

    var makeDriver = type.GetMethod("Driver", BindingFlags.Static | BindingFlags.Public);
    if (makeDriver == null)
      throw new InvalidOperationException(
        $"Expected to find a public static method named `Driver` on `{configuration.Browser.DriverFactory}`");

    if (!typeof(IWebDriver).IsAssignableFrom(makeDriver.ReturnType))
      throw new InvalidOperationException(
        $"Expected return type deriving from `IWebDriver` from `Driver` on `{configuration.Browser.DriverFactory}`");

    var param = makeDriver.GetParameters();
    if (param.Length != 2 || 
        !typeof(WebDriverConfig).IsAssignableFrom(param[0].ParameterType) ||
        !typeof(RuntimeConfig).IsAssignableFrom(param[1].ParameterType))
      throw new InvalidOperationException(
        $"Expected parameter type of `WebDriverConfig` for `Driver` on `{configuration.Browser.DriverFactory}`");

    return (IWebDriver)makeDriver.Invoke(null, new object?[] { configuration, runtime })!;
  }
}
