using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Decorates a class and performs all necessary setup and execution of a selenium test
  /// </summary>
  public class RequiresWebBrowserAttribute : 
    Attribute, 
    ITestAction, 
    IManageLogging,
    IRequireServicesCollection,
    IRequireLogger
  {
    /// <summary>
    /// Screenshot directory
    /// </summary>
    public const string ScreenshotDirectoryName = "Screenshots";
    public const string DownloadsDirectoryName = "Downloads";

    private static readonly Type WebSiteType = typeof(WebSite);
    private static readonly Type RequiresWebPage = typeof(IRequireWebPage<,>);

    private IAuthenticationHelper authHelper;
    private ILogger log;

    private IWebDriver webDriver;
    private IServiceScope scope;
    private IServiceProvider provider;

    /// <inheritdoc />
    public void BeforeTest(ITest test)
    {
      var services = this.GetServiceCollection();
      this.AddLogging();
      var lf = this.GetLoggerFactory();
      log = this.CreateLogger();
      var (siteType, pageType) = GetFixtureSite(test);
      
      var authentication = test.Fixture as IRequireAuthentication;
      var configProvider = test.Fixture as IProvideServiceCollectionConfiguration;

      configProvider?.Configure(services);

      services.TryAddScoped(
        typeof(IAuthenticationHelper),
        authentication != null ? typeof(AuthenticationHelper) : typeof(NoOpAuthenticationHelper));

      services.TryAddScoped(
        sp => {
          var auth = sp.GetRequiredService<IAuthenticationHelper>();
          var cfg = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;
          var dd = GetCleanDirectory(
            RequireDirectoryExtensions.DownloadDirectoryPropertyKey,
            test.Fixture as IRequireDownloadDirectory,
            DownloadsDirectoryName);

          var (wd, initializationException) =
            WebDriverFactory.CreateAndInitialize(cfg, dd, (c, drv) => auth.PerformAuth(drv, c));

          initializationException?.Throw();
          return wd;
        }
      );

      provider = this.BuildServiceProvider();
      scope = provider.CreateScope();
      
      var configHolder = test.Fixture as IRequireCurrentWebDriverConfig;
      var screenshots = test.Fixture as IRequireScreenshots;

      var config = scope.ServiceProvider.GetRequiredService<IOptions<WebDriverConfig>>().Value;
      if (configHolder != null)
        configHolder.Config = config;
        
      /* lazy initialized */
      webDriver = scope.ServiceProvider.GetRequiredService<IWebDriver>();
      var requiresPageType = RequiresWebPage.MakeGenericType(siteType, pageType);
      var page = (IWebPage) scope.ServiceProvider.GetRequiredService(pageType);
      var site = (WebSite) page.Parent;

      requiresPageType.GetProperty("Site").SetValue(test.Fixture, site);
      requiresPageType.GetProperty("Page").SetValue(test.Fixture, page);

      site.PageFactory = t => (IWebPage) scope.ServiceProvider.GetRequiredService(t);

      if (screenshots != null)
        page.OnPageLoaded += (_, __) => { SaveScreenshotForEachTab(test, "_pre"); };
    }

    /// <inheritdoc />
    public void AfterTest(ITest test)
    {
      var currentContext = TestContext.CurrentContext;
      if (currentContext.Result.Outcome.Status == TestStatus.Failed)
        SafelyTry.Action(() => SaveScreenshotForEachTab(test, "_post"));

      webDriver?.Close();
      webDriver?.Quit();
      webDriver?.Dispose();
      scope?.Dispose();
      TestExecutionContext.CurrentContext.ClearSuiteProperties();
    }

    /// <summary>
    /// What should this attribute target
    /// </summary>
    public ActionTargets Targets => ActionTargets.Test;
    
    private (Type, Type) GetFixtureSite(ITest testFixture)
    {
      var i = testFixture.Fixture?
        .GetType()
        .GetInterfaces()
        .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == RequiresWebPage);
      
      if (i == null)
        throw new Exception(
          $"Test should implement interface '{RequiresWebPage}' instead of using the attribute '{GetType()}'");

      var types = i.GetGenericArguments().ToArray();
      return (types[0], types[1]);
    }

    private void SaveScreenshotForEachTab(ITest test, string suffix)
    {
      if (test.Fixture is not IRequireScreenshots requiree)
        return;

      var fileName = CleanPath($"{test.FullName}{suffix}.png");
      var dir = GetCleanDirectory(
        RequireDirectoryExtensions.ScreenshotDirectoryPropertyKey,
        requiree,
        ScreenshotDirectoryName);

      var path = Path.Combine(dir, fileName);
      webDriver.SaveScreenshotForEachTab(path, log);
      log.LogInformation("Saving screen shot for test '{0}'", test.FullName);

      if (!string.IsNullOrEmpty(path))
        log.LogInformation(
          $"##vso[artifact.upload containerfolder=SeleniumScreenshots;artifactname={fileName};]" +
          Path.Combine(Directory.GetCurrentDirectory(), path));
    }

    private static string CleanPath(string path)
    {
      var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
      var r = new Regex($"[{Regex.Escape(regexSearch)}]");
      return r.Replace(path, "");
    }

    /// <inheritdoc />
    public string GetCleanDirectory(string context, object requiree, string relativeDir)
    {
      if (requiree == null)
        return null;

      var directoryInfo = new DirectoryInfo(
        Path.Combine(
          TestContext.CurrentContext.TestDirectory, 
          relativeDir));

      if (directoryInfo.Exists)
      {
        directoryInfo.Delete(true);
        var count = 0;
        while (directoryInfo.Exists)
        {
          directoryInfo.Refresh();
          if (++count == 5)
            break;
        }
      }

      directoryInfo.Create();
      TestExecutionContext.CurrentContext.SetSuiteProperty(context, directoryInfo.FullName);
      return directoryInfo.FullName;
    }
  }
}