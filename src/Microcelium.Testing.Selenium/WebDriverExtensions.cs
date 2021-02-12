using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microcelium.Testing.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using NetCookie = System.Net.Cookie;
using SeleniumCookie = OpenQA.Selenium.Cookie;

namespace Microcelium
{
  /// <summary>
  /// A collection of selenium extensions
  /// </summary>
  public static class WebDriverExtensions
  {
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan TenSeconds = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ThirtySeconds = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSite"></typeparam>
    /// <param name="webDriver"></param>
    /// <param name="baseAddress"></param>
    /// <returns></returns>
    public static TSite UsingSite<TSite>(this IWebDriver webDriver, WebDriverConfig config, ILogger log) where TSite : IWebSite, new()
    {
      var site = new TSite();
      site.Initialize(webDriver, config, log);
      return site;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="webDriver"></param>
    /// <param name="filePath"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static string SaveScreenshotForEachTab(this IWebDriver webDriver, string filePath, ILogger log)
      => new BrowserScreenshotCapturer(webDriver, log).SaveScreenshotForEachTab(filePath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="webDriver"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="site"></param>
    public static void ImportCookies(this IWebDriver webDriver, CookieContainer cookieContainer, Uri site)
      => webDriver.ImportCookies(cookieContainer.GetCookies(site).Cast<NetCookie>());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="webDriver"></param>
    /// <param name="cookies"></param>
    public static void ImportCookies(this IWebDriver webDriver, IEnumerable<NetCookie> cookies)
      => cookies.ToList()
        .ForEach(
          cookie => webDriver.Manage()
            .Cookies.AddCookie(new SeleniumCookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null)));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void ScrollTo(this IWebDriver driver, int x, int y)
      => driver.ExecuteScript<object>($"window.scrollTo({x}, {y});");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="by"></param>
    /// <param name="timout"></param>
    /// <returns></returns>
    public static IWebElement ElementExists(this IWebDriver driver, By by, TimeSpan? timout = null)
    {
      try
      {
        return driver.WaitUntil(ExpectedConditions.ElementExists(by), timout ?? TimeSpan.Zero);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// This does everything I could possibly think of to ensure we wait for all AJAX
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="seconds">the maximum amount of time to wait until an exception is thrown</param>
    /// <returns></returns>
    public static bool DefinitivelyWaitForAnyAjax(this IWebDriver driver, ILogger log, TimeSpan? seconds = null)
    {
      var result = driver.WaitForJavascriptResult("(window.jQuery || { active: 0 } ).active", 0, log, seconds )
        && driver.WaitForJavascriptResult("((window.jQuery.ajax || { ajax: null } ).ajax || { active: 0 }).active", 0, log, seconds )
        && driver.WaitForJavascriptResult("$ && $('.dataTables_processing').is(':visible') === false ? 0 : 1", 0, log, seconds )
        && driver.WaitForJavascriptResult("document.readyState", "complete", log, seconds ?? DefaultTimeout);
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="driver"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    public static TResult ExecuteScript<TResult>(this IWebDriver driver, string script)
    {
      var js = (IJavaScriptExecutor)driver;
      var executeScript = js.ExecuteScript(script);
      if (executeScript == null)
        return default(TResult);
      return (TResult)Convert.ChangeType(executeScript, typeof(TResult));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="directory"></param>
    /// <param name="fileMask"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static FileInfo WaitForFileDownload(this IWebDriver driver, string directory, string fileMask, ILogger log, TimeSpan? timeout = null)
      => new DownloadHelper(driver, log).WaitForFileDownload(directory, fileMask, timeout ?? TimeSpan.FromSeconds(60));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="driver"></param>
    /// <param name="condition"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static T WaitUntil<T>(this IWebDriver driver, Func<IWebDriver, T> condition, TimeSpan timeout)
      => new WebDriverWait(driver, timeout).Until(condition);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="driver"></param>
    /// <param name="javascript"></param>
    /// <param name="expectedResult"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static bool WaitForJavascriptResult<TResult>(this IWebDriver driver, string javascript, TResult expectedResult, ILogger log, TimeSpan? timeout = null)
      => driver.WaitUntil(Javascript.FunctionResult(javascript, log).Matches(expectedResult), timeout ?? TenSeconds);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="driver"></param>
    /// <param name="javascript"></param>
    /// <param name="expectedResult"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static bool WaitForDifferentJavascriptResult<TResult>(this IWebDriver driver, string javascript, TResult expectedResult, ILogger log, TimeSpan? timeout = null)
      => driver.WaitUntil(Javascript.FunctionResult(javascript, log).Matches(expectedResult, false), timeout ?? TenSeconds);

    /// <summary>
    /// Finds an alert window
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static IAlert GetAlert(this IWebDriver driver, ILogger log, TimeSpan? timeout = null)
    {
      log.LogInformation("Looking for alert...");
      return driver.WaitUntil(
        d =>
          {
            try
            {
              var alert = d.SwitchTo().Alert();
              log.LogInformation("Found alert with text: {0}", alert.Text);
              return alert;
            }
            catch (NoAlertPresentException) { return null; }
          },
        timeout ?? TenSeconds);
    }

    /// <summary>
    /// Navigates the driver to the specific URI
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="relativeUrl">the relative target</param>
    /// <returns></returns>
    public static IWebDriver GoToRelativeUrl(this IWebDriver driver, string relativeUrl)
    {
      driver.Navigate().GoToUrl(new Uri(new Uri(driver.Url), relativeUrl));
      return driver;
    }

    [Obsolete("Please use CSS Selectors")]
    public static IWebElement FindElementByXPath(this IWebDriver driver, string xpath)
      => driver.FindElement(By.XPath(xpath));

    [Obsolete("Please use CSS Selectors")]
    public static IWebElement FindEleByXPath(this IWebDriver driver, string xpath)
      => driver.FindElement(By.XPath(xpath));

    [Obsolete("Please use CSS Selectors")]
    public static IWebElement WaitForElementToBeVisible(this IWebDriver driver, string xpath)
      => driver.WaitForElementToBeVisible(By.XPath(xpath));

    /// <summary>
    /// Waits for an Element to be visible or times out
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="by">the element's selector</param>
    /// <returns></returns>
    public static IWebElement WaitForElementToBeVisible(this IWebDriver driver, By by)
      => driver.WaitUntil(ExpectedConditions.ElementIsVisible(@by), ThirtySeconds);

    /// <summary>
    /// Waits for an Element to be clickable or times out
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="by">the element's selector</param>
    /// <returns></returns>
    public static IWebElement WaitForElementToBeClickable(this IWebDriver driver, By by)
      => driver.WaitUntil(ExpectedConditions.ElementToBeClickable(@by), ThirtySeconds);

    /// <summary>
    /// Waits for an Element to be hidden or times out
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="by">the element's selector</param>
    /// <returns></returns>
    public static bool WaitForElementToBeHidden(this IWebDriver driver, By by)
      => driver.WaitUntil(ExpectedConditions.InvisibilityOfElementLocated(@by), ThirtySeconds);
  }
}