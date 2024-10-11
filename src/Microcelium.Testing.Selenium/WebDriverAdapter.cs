using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using NetCookie = System.Net.Cookie;
using SeleniumCookie = OpenQA.Selenium.Cookie;

namespace Microcelium.Testing.Selenium;

public sealed class WebDriverAdapter : IWebDriverExtensions
{
  private readonly IWebDriver driver;
  private readonly WebDriverConfig config;
  private readonly ILoggerFactory loggerFactory;
  private readonly ILogger<WebDriverAdapter> log;

  private readonly BrowserScreenshotCapturer? capturer;
  private bool disposed;

  public WebDriverAdapter(
    IWebDriver driver, 
    WebDriverConfig config,
    ILoggerFactory loggerFactory)
  {
    this.driver = driver;
    this.config = config;
    this.loggerFactory = loggerFactory;
    this.log = loggerFactory.CreateLogger<WebDriverAdapter>();
    this.capturer = new BrowserScreenshotCapturer(this, loggerFactory);
  }
  
  public string Url
  {
    get => driver.Url;
    set => driver.Url = value;
  }

  public IWebElement FindElement(By by) => driver.FindElement(by);
  public ReadOnlyCollection<IWebElement> FindElements(By by) => driver.FindElements(by);
  public void Close() { driver.Close(); }
  public void Quit() { driver.Quit(); }
  public IOptions Manage() => driver.Manage();
  public INavigation Navigate() => driver.Navigate();
  public ITargetLocator SwitchTo() => driver.SwitchTo();
  public string Title => driver.Title;
  public string PageSource => driver.PageSource;
  public string CurrentWindowHandle => driver.CurrentWindowHandle;
  public ReadOnlyCollection<string> WindowHandles => driver.WindowHandles;
  public WebDriverConfig Config => config;
  public ILoggerFactory LoggerFactory => loggerFactory;
  public Screenshot GetScreenshot() => ((ITakesScreenshot)driver).GetScreenshot();

  public void Dispose() 
  {
    if (disposed)
      return;

    disposed = true;
    driver.Close();
    driver.Quit(); /* calls dispose */
    GC.SuppressFinalize(this);
  }

  ~WebDriverAdapter()
  {
    Dispose();
  }

  /// <summary>
  /// </summary>
  /// <param name="directoryPath"></param>
  /// <returns></returns>
  public string SaveScreenshotForEachTab(string directoryPath) => capturer.SaveScreenshotForEachTab(directoryPath);

  ///// <summary>
  ///// </summary>
  ///// <typeparam name="TSite"></typeparam>
  ///// <param name="baseAddress"></param>
  ///// <returns></returns>
  //public TSite UsingSite<TSite>(WebDriverConfig config)
  //  where TSite : IWebSite, new()
  //{
  //  var site = new TSite();
  //  site.Initialize(driver, config, log);
  //  return site;
  //}

  /// <summary>
  /// </summary>
  /// <param name="cookieContainer"></param>
  /// <param name="site"></param>
  public void ImportCookies(CookieContainer cookieContainer, Uri site) =>
    ImportCookies(cookieContainer.GetCookies(site));

  /// <summary>
  /// </summary>
  /// <param name="cookies"></param>
  public void ImportCookies(IEnumerable<NetCookie> cookies) =>
    cookies.ToList()
      .ForEach(
        c => {
          //c.Domain = c.Domain.Contains("localhost") ? null : c.Domain;
          driver.Manage()
            .Cookies
            .AddCookie(new SeleniumCookie(c.Name, c.Value, c.Domain, c.Path, null));
        });

  /// <inheritdoc />
  public void ExportCookies(CookieContainer container, Uri? site = null)
  {
    var cookies = site == null
      ? driver.Manage().Cookies.AllCookies.ToList()
      : driver.Manage().Cookies.AllCookies
        .Where(x => x.Domain.Equals(site.Host, StringComparison.CurrentCultureIgnoreCase))
        .ToList();

    cookies.ForEach(
      x => container.Add(new NetCookie(x.Name, x.Value, x.Path, x.Domain)));
  }

  /// <summary>
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  public void ScrollTo(int x, int y) =>
    ExecuteScript<int>($"window.scrollTo({x}, {y}); return 1;");

  /// <summary>
  /// </summary>
  /// <param name="by"></param>
  /// <returns></returns>
  public IWebElement? ElementExists(By by)
  {
    try
    {
      return WaitUntil(ExpectedConditions.ElementExists(by));
    }
    catch
    {
      return null;
    }
  }

  /// <summary>
  ///   This does everything I could possibly think of to ensure we wait for all AJAX
  /// </summary>
  /// <param name="seconds">the maximum amount of time to wait until an exception is thrown</param>
  /// <returns></returns>
  public bool DefinitivelyWaitForAnyAjax(TimeSpan? seconds = null)
  {
    var result = 
      WaitForJavascriptResult("(window.jQuery || { active: 0 } ).active", 0) &&
      WaitForJavascriptResult("((window.jQuery.ajax || { ajax: null } ).ajax || { active: 0 }).active", 0) &&
      WaitForJavascriptResult("$ && $('.dataTables_processing').is(':visible') === false ? 0 : 1", 0) &&
      WaitForJavascriptResult("document.readyState", "complete");

    return result;
  }

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="script"></param>
  /// <returns></returns>
  public TResult ExecuteScript<TResult>(string script) where TResult : struct, IConvertible
  {
    var js = (IJavaScriptExecutor)driver;
    var executeScript = js.ExecuteScript(script);
    if (executeScript == null)
      return default;

    return (TResult)Convert.ChangeType(executeScript, typeof(TResult));
  }

  /// <summary>
  /// </summary>
  /// <param name="directory"></param>
  /// <param name="fileMask"></param>
  /// <returns></returns>
  public FileInfo? WaitForFileDownload(
    string directory,
    string fileMask) =>
    new DownloadHelper(config.Timeout.Download, loggerFactory)
      .WaitForFileDownload(new DirectoryInfo(directory), fileMask);

  /// <summary>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="condition"></param>
  /// <returns></returns>
  public T WaitUntil<T>(Func<IWebDriver, T> condition) =>
    new WebDriverWait(driver, config.Timeout.Implicit).Until(condition);

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="javascript"></param>
  /// <param name="expectedResult"></param>
  /// <returns></returns>
  public bool WaitForJavascriptResult<TResult>(string javascript, TResult expectedResult) 
    where TResult : IConvertible =>
    WaitUntil(
      Javascript.FunctionResult(javascript, loggerFactory).Matches(expectedResult));

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="javascript"></param>
  /// <param name="expectedResult"></param>
  /// <returns></returns>
  public bool WaitForDifferentJavascriptResult<TResult>(string javascript, TResult expectedResult) 
    where TResult : IConvertible =>
    WaitUntil(
      Javascript.FunctionResult(javascript, loggerFactory).DoesNotMatch(expectedResult));

  /// <summary>
  ///   Finds an alert window
  /// </summary>
  /// <returns></returns>
  public IAlert? GetAlert()
  {
    log.LogInformation("Looking for alert...");
    return WaitUntil(
      d => {
        try
        {
          var alert = d.SwitchTo().Alert();
          log.LogInformation("Found alert with text: {alert}", alert.Text);
          return alert;
        }
        catch (NoAlertPresentException)
        {
          return null;
        }
      });
  }

  /// <summary>
  ///   Navigates the driver to the specific URI
  /// </summary>
  /// <param name="relativeUrl">the relative target</param>
  /// <returns></returns>
  public IWebDriver GoToRelativeUrl(string relativeUrl)
  {
    driver.Navigate().GoToUrl(new Uri(new Uri(driver.Url), relativeUrl));
    return driver;
  }

  /// <summary>
  ///   Waits for an Element to be visible or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  public IWebElement WaitForElementToBeVisible(By by) =>
    WaitUntil(ExpectedConditions.ElementIsVisible(by));

  /// <summary>
  ///   Waits for an Element to be clickable or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  public IWebElement WaitForElementToBeClickable(By by) =>
    WaitUntil(ExpectedConditions.ElementToBeClickable(by));

  /// <summary>
  ///   Waits for an Element to be hidden or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  public bool WaitForElementToBeHidden(By by) =>
    WaitUntil(ExpectedConditions.InvisibilityOfElementLocated(by));

  public Type DriverType => driver.GetType();
}