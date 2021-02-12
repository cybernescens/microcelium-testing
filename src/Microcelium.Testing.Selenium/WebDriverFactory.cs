using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  ///   Builds the WebDriver
  /// </summary>
  public static class WebDriverFactory
  {
    internal static readonly IDictionary<string, Func<WebDriverConfig, string, IWebDriver>> DriverBuilders =
      new Dictionary<string, Func<WebDriverConfig, string, IWebDriver>> {
        {
          typeof(ChromeDriver).FullName,
          (config, dd) => new ChromeDriver(
            CreateChromeDriverService(),
            config.GetChromeOptions(null, dd),
            config.BrowserTimeout)
        }
      };

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="builder"></param>
    public static void AddDriverBuilder(string type, Func<WebDriverConfig, string, IWebDriver> builder) =>
      DriverBuilders.Add(type, builder);

    /// <summary>
    ///   Creates a web driver
    /// </summary>
    /// <param name="browserConfig"></param>
    /// <returns></returns>
    public static IWebDriver Create(WebDriverConfig browserConfig, string downloadDirectory = null)
    {
      if (browserConfig.BrowserType == null || !DriverBuilders.ContainsKey(browserConfig.BrowserType))
        throw new NotImplementedException($"No browser configured for type '{browserConfig.BrowserType}'");

      var builder = DriverBuilders[browserConfig.BrowserType];
      var webDriver = builder(browserConfig, downloadDirectory);

      webDriver.Manage().Timeouts().ImplicitWait = browserConfig.ImplicitTimeout;
      webDriver.Manage().Timeouts().PageLoad = browserConfig.PageLoadTimeout;

      return webDriver;
    }

    /// <summary>
    ///   Initializes a Selenium WebDriver in a Lazy and Thread-safe way
    /// </summary>
    /// <param name="browserConfig"></param>
    /// <param name="initializeBrowser"></param>
    /// <returns></returns>
    public static (IWebDriver Driver, ExceptionDispatchInfo InitializationException) CreateAndInitialize(
      WebDriverConfig browserConfig,
      string downloadDirectory,
      Action<WebDriverConfig, IWebDriver> initializeBrowser)
    {
      var driver = Create(browserConfig, downloadDirectory);
      ExceptionDispatchInfo ie = null;
      
      try { initializeBrowser(browserConfig, driver); }
        catch (Exception e) { ie = ExceptionDispatchInfo.Capture(e); }

      return (driver, ie);
    }

    private static ChromeDriverService CreateChromeDriverService()
    {
      string GetCurrentDirectory()
      {
        var assembly = typeof(WebDriverFactory).Assembly;
        var directoryName = Path.GetDirectoryName(assembly.Location);
        if (AppDomain.CurrentDomain.ShadowCopyFiles)
          directoryName = Path.GetDirectoryName(new Uri(assembly.CodeBase).LocalPath);

        return directoryName;
      }

      var currentDirectory = GetCurrentDirectory();
      var path = File.Exists(Path.Combine(currentDirectory, "chromedriver.exe")) ? currentDirectory : ".";
      var chromeDriverService = ChromeDriverService.CreateDefaultService(path);
      chromeDriverService.HideCommandPromptWindow = true;
      return chromeDriverService;
    }
  }
}