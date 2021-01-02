using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Microcelium.Testing.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace Microcelium.Testing.NUnit.Selenium
{
  public class RequiresWebBrowserAttribute : Attribute, ITestAction, IRequireLogger
  {
    public const string ScreenshotDirectoryName = "Screenshots";

    private static readonly Type RequiresWebType = typeof(IRequireWebSite<>);
    private static readonly Type RequiresWebTypeConstraintType = typeof(Page<>);
    private IAuthenticationHelper authHelper;

    private IWebDriver webDriver;
    private ILogger log;

    /// <inheritdoc />
    public void BeforeTest(ITest test)
    {
      this.log = this.CreateLogger();
      var pageType = GetFixturePage(test);

      var configBuilder = test.Fixture as IProvideWebDriverConfigBuilder;
      var configHolder = test.Fixture as IRequireCurrentWebDriverConfig;
      var authentication = test.Fixture as IRequireAuthentication;
      var screenshots = test.Fixture as IRequireScreenshots;
      var downloads = test.Fixture as IRequireDownloadDirectory;

      var config =
        configBuilder?.Builder.Build()
        ?? WebDriver
          .Configure(
            builder =>
              {
                builder.WithDefaultOptions();
                if (downloads != null)
                  builder.DownloadDirectory(downloads.DownloadDirectory);
                return builder;
              }, log)
          .Build();

      if (configHolder != null)
        configHolder.Config = config;
      
      /* lazy initialized */
      authHelper = new AuthenticationHelper(config, log);

      var init = authentication != null
        ? (Action<IWebDriverConfig, IWebDriver, IAuthenticationHelper>) AuthenticatedInitialize
        : Initialize;

      ExceptionDispatchInfo initializationException = null;
      (webDriver, initializationException) = WebDriverFactory.CreateAndInitialize(config, (cfg, drv) => init(cfg, drv, authHelper));
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

      SafelyTry.Action(() => webDriver.Close());
      SafelyTry.Action(() => webDriver.Quit());
      SafelyTry.Dispose(webDriver);
    }

    public ActionTargets Targets => ActionTargets.Default;

    private Page GetPage(Type pageType, IWebDriverConfig config)
      => (Page)webDriver.UsingSite<Site>(config, log).NavigateToPage(pageType);

    private Type GetFixturePage(ITest testFixture)
    {
      foreach (var i in testFixture.Fixture.GetType().GetInterfaces())
      {
        if (!i.IsGenericType)
          continue;
        if (i.GetGenericTypeDefinition() == RequiresWebType)
          return i.GetGenericArguments()[0];
      }

      throw new Exception(
        $"Test should implement interface '{RequiresWebType}' instead of using the attribute '{GetType()}'");
    }

    private static void Initialize(IWebDriverConfig config, IWebDriver driver, IAuthenticationHelper authHelper)
      => driver.Navigate().GoToUrl(config.BaseUrl);

    private void AuthenticatedInitialize(IWebDriverConfig config, IWebDriver driver, IAuthenticationHelper authHelper)
    {
      void ApplyCookiesToWebDriverAndNavigate()
      {
        var cc = authHelper.AuthCookies;
        driver.Manage().Cookies.DeleteAllCookies();
        cc.GetCookies(config.BaseUrl)
          .Cast<System.Net.Cookie>()
          .Select(
            c =>
              {
                if (c.Domain.Contains("localhost"))
                  c.Domain = null;
                return c;
              })
          .ToList()
          .ForEach(
            cookie => driver.Manage()
              .Cookies.AddCookie(new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null)));

        driver.Navigate().GoToUrl(config.BaseUrl);
      }

      driver.GoToRelativeUrl(config.BaseUrl + config.RelativeMicroceliumLogoPath);
      ApplyCookiesToWebDriverAndNavigate();
      driver.WaitForElementToBeVisible(By.LinkText("Logout"));
      driver.DefinitivelyWaitForAnyAjax(log, config.PageLoadTimeout);
    }

    private void SaveScreenshotForEachTab(TestContext currentContext, string suffix)
    {
      var fileName = CleanPath($"{currentContext.Test.FullName}{suffix}.png");
      var path = $"{ScreenshotDirectoryName}\\{fileName}";
      webDriver.SaveScreenshotForEachTab(path, log);
      log.LogInformation("Saving screen shoot for test '{0}'", currentContext.Test.FullName);

      if (!string.IsNullOrEmpty(path))
      {
        log.LogInformation(
            $"##teamcity[publishArtifacts '{Path.Combine(Directory.GetCurrentDirectory(), path)} => SeleniumScreenshots']");
      }
    }

    private static string CleanPath(string path)
    {
      var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
      var r = new Regex($"[{Regex.Escape(regexSearch)}]");
      return r.Replace(path, "");
    }
  }
}