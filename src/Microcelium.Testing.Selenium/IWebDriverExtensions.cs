using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Cookie = System.Net.Cookie;

namespace Microcelium.Testing.Selenium;

public interface IWebDriverExtensions : IWebDriver
{
  /// <summary>
  /// </summary>
  WebDriverConfig Config { get; }

  /// <summary>
  /// </summary>
  ILoggerFactory LoggerFactory { get; }

  /// <summary>
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  string SaveScreenshotForEachTab(string filePath);

  /// <summary>
  /// </summary>
  /// <param name="cookieContainer"></param>
  /// <param name="site"></param>
  void ImportCookies(CookieContainer cookieContainer, Uri site);

  /// <summary>
  /// </summary>
  /// <param name="cookies"></param>
  void ImportCookies(IEnumerable<Cookie> cookies);

  /// <summary>
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  void ScrollTo(int x, int y);

  /// <summary>
  /// </summary>
  /// <param name="by"></param>
  /// <returns></returns>
  IWebElement? ElementExists(By by);

  /// <summary>
  ///   This does everything I could possibly think of to ensure we wait for all AJAX
  /// </summary>
  /// <param name="seconds">the maximum amount of time to wait until an exception is thrown</param>
  /// <returns></returns>
  bool DefinitivelyWaitForAnyAjax(TimeSpan? seconds = null);

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="script"></param>
  /// <returns></returns>
  TResult ExecuteScript<TResult>(string script) where TResult : struct, IConvertible;

  /// <summary>
  /// </summary>
  /// <param name="directory"></param>
  /// <param name="fileMask"></param>
  /// <returns></returns>
  FileInfo? WaitForFileDownload(
    string directory,
    string fileMask);

  /// <summary>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="condition"></param>
  /// <returns></returns>
  T WaitUntil<T>(Func<IWebDriver, T> condition);

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="javascript"></param>
  /// <param name="expectedResult"></param>
  /// <returns></returns>
  bool WaitForJavascriptResult<TResult>(string javascript, TResult expectedResult) 
    where TResult : IConvertible;

  /// <summary>
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  /// <param name="javascript"></param>
  /// <param name="expectedResult"></param>
  /// <returns></returns>
  bool WaitForDifferentJavascriptResult<TResult>(string javascript, TResult expectedResult) 
    where TResult : IConvertible;

  /// <summary>
  ///   Finds an alert window
  /// </summary>
  /// <returns></returns>
  IAlert? GetAlert();

  /// <summary>
  ///   Navigates the driver to the specific URI
  /// </summary>
  /// <param name="relativeUrl">the relative target</param>
  /// <returns></returns>
  IWebDriver GoToRelativeUrl(string relativeUrl);

  /// <summary>
  ///   Waits for an Element to be visible or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  IWebElement WaitForElementToBeVisible(By by);

  /// <summary>
  ///   Waits for an Element to be clickable or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  IWebElement WaitForElementToBeClickable(By by);

  /// <summary>
  ///   Waits for an Element to be hidden or times out
  /// </summary>
  /// <param name="by">the element's selector</param>
  /// <returns></returns>
  bool WaitForElementToBeHidden(By by);

  /// <summary>
  /// Gets the inner driver type
  /// </summary>
  Type DriverType { get; }

  /// <summary>
  /// Gets the Driver Screenshot
  /// </summary>
  /// <returns></returns>
  Screenshot GetScreenshot();
}