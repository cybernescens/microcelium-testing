using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Microcelium.Testing.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Decorates a class and performs all necessary setup and execution of a selenium test
  /// </summary>
  public class RequiresWebBrowserAttribute : 
    Attribute, 
    ITestAction, 
    IRequireLogger, 
    IManageServiceCollection,
    IRequireServicesCollection
  {
    /// <summary>
    /// Screenshot directory
    /// </summary>
    public const string ScreenshotDirectoryName = "Screenshots";

    private static readonly Type RequiresWebType = typeof(IRequireWebSite<>);
    private IAuthenticationHelper authHelper;
    private ILogger log;

    private IWebDriver webDriver;
    private IServiceScope scope;

    /// <inheritdoc />
    public void BeforeTest(ITest test)
    {
      var services = this.GetServiceCollection();
      var lf = this.GetLoggerFactory();
      log = this.CreateLogger();
      var pageType = GetFixturePage(test);
      
      var authentication = test.Fixture as IRequireAuthentication;
      var configProvider = test.Fixture as IProvideServiceCollectionConfiguration;

      configProvider?.Configure(services);
      services.AddWebDriverConfig();

      if (authentication != null)
        services.TryAddScoped<IAuthenticationHelper, AuthenticationHelper>();

      var sp = this.BuildServiceProvider();
      this.scope = sp.CreateScope();

      var configHolder = test.Fixture as IRequireCurrentWebDriverConfig;
      var screenshots = test.Fixture as IRequireScreenshots;
      var downloads = test.Fixture as IRequireDownloadDirectory;

      var config = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;
      if (configHolder != null)
        configHolder.Config = config;

      /* lazy initialized */
      authHelper = sp.GetRequiredService<IAuthenticationHelper>();

      var (wd, initializationException) =
        authentication != null
          ? WebDriverFactory.CreateAndInitialize(config, downloads?.DownloadDirectory, (cfg, drv) => authHelper.PerformAuth(drv, cfg))
          : WebDriverFactory.CreateAndInitialize(config, downloads?.DownloadDirectory, (cfg, drv) => drv.Navigate().GoToUrl(cfg.GetBaseUrl()));

      webDriver = wd;
      initializationException?.Throw();

      var requiresWebForPageType = RequiresWebType.MakeGenericType(pageType);
      var page = GetPage(pageType, config);

      requiresWebForPageType.GetProperty("StartPage")?.SetValue(test.Fixture, page);

      if (screenshots != null)
      {
        page.WaitForPageToLoad();
        SaveScreenshotForEachTab(TestContext.CurrentContext, "_pre");
      }
    }

    /// <inheritdoc />
    public void AfterTest(ITest testDetails)
    {
      var currentContext = TestContext.CurrentContext;
      if (currentContext.Result.Outcome.Status == TestStatus.Failed)
        SafelyTry.Action(() => SaveScreenshotForEachTab(currentContext, "_post"));

      webDriver?.Close();
      webDriver?.Quit();
      webDriver?.Dispose();
      scope?.Dispose();
    }

    /// <summary>
    /// What should this attribute target
    /// </summary>
    public ActionTargets Targets => ActionTargets.Default;

    private Page GetPage(Type pageType, WebDriverConfig config) =>
      (Page) webDriver.UsingSite<Site>(config, log).NavigateToPage(pageType);

    private Type GetFixturePage(ITest testFixture)
    {
      foreach (var i in testFixture.Fixture?.GetType().GetInterfaces() ?? Array.Empty<Type>())
      {
        if (!i.IsGenericType)
          continue;

        if (i.GetGenericTypeDefinition() == RequiresWebType)
          return i.GetGenericArguments()[0];
      }

      throw new Exception(
        $"Test should implement interface '{RequiresWebType}' instead of using the attribute '{GetType()}'");
    }

    private void SaveScreenshotForEachTab(TestContext currentContext, string suffix)
    {
      var fileName = CleanPath($"{currentContext.Test.FullName}{suffix}.png");
      var path = $"{ScreenshotDirectoryName}\\{fileName}";
      webDriver.SaveScreenshotForEachTab(path, log);
      log.LogInformation("Saving screen shoot for test '{0}'", currentContext.Test.FullName);

      if (!string.IsNullOrEmpty(path))
        log.LogInformation(
          $"##teamcity[publishArtifacts '{Path.Combine(Directory.GetCurrentDirectory(), path)} => SeleniumScreenshots']");
    }

    private static string CleanPath(string path)
    {
      var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
      var r = new Regex($"[{Regex.Escape(regexSearch)}]");
      return r.Replace(path, "");
    }
  }
}