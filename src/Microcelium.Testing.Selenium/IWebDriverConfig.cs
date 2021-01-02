using System;
using System.Drawing;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  ///   Selenium Driver Configuration Options
  /// </summary>
  public interface IWebDriverConfig
  {
    // <summary>
    /// Configuration parameter:
    /// <c>webdriver.browser.type</c>
    /// </summary>
    string BrowserType { get; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.type</c>
    /// </summary>
    Size BrowserSize { get; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.runheadless</c>
    /// </summary>
    bool RunHeadless { get; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.pageload</c>
    /// </summary>
    TimeSpan PageLoadTimeout { get; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.implicitwait</c>
    ///   This really should be zero
    /// </summary>
    TimeSpan ImplicitTimeout { get; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.browser</c>
    ///   Timeout waiting for browser to respond. Default to 60 seconds
    /// </summary>
    TimeSpan BrowserTimeout { get; }

    /// <summary>
    ///   Gets the configured <see cref="ChromeOptions" />
    /// </summary>
    ChromeOptions ChromeOptions { get; }

    /// <summary>
    /// The Base URL of the site the driver will be working with
    /// </summary>
    Uri BaseUrl { get; }

    /// <summary>
    /// If authentication is required, the username
    /// </summary>
    string Username { get; }

    /// <summary>
    /// If authentication is required, the password
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Relative Redirect URL after logging in
    /// </summary>
    string RelativeLoginUrl { get; }

    /// <summary>
    /// Relative path to an inteligenz logo, should be a path
    /// that requires no authentication
    /// </summary>
    string RelativeMicroceliumLogoPath { get; }
  }
}