using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  /// Selenium Driver Configuration Options
  /// </summary>
  [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
  public class WebDriverConfig : IWebDriverConfig
  {
    /// <inheritdoc />
    internal WebDriverConfig() { }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.type</c>
    /// </summary>
    public string BrowserType { get; internal set; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.type</c>
    /// </summary>
    public Size BrowserSize { get; internal set; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.runheadless</c>
    /// </summary>
    public bool RunHeadless { get; internal set; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.pageload</c>
    /// </summary>
    public TimeSpan PageLoadTimeout { get; internal set; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.implicitwait</c>
    ///   This really should be zero
    /// </summary>
    public TimeSpan ImplicitTimeout { get; internal set; }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.browser</c>
    ///   Timeout waiting for browser to respond. Default to 60 seconds
    /// </summary>
    public TimeSpan BrowserTimeout { get; internal set; }

    /// <summary>
    /// Gets the configured <see cref="ChromeOptions"/>
    /// </summary>
    public ChromeOptions ChromeOptions { get; internal set; }

    /// <summary>
    /// This site Base URI
    /// </summary>
    public Uri BaseUrl { get; internal set; }

    /// <summary>
    /// If authentication is required, the username
    /// </summary>
    public string Username { get; internal set; }

    /// <summary>
    /// If authentication is required, the password
    /// </summary>
    public string Password { get; internal set; }

    /// <summary>
    /// Relative Redirect URL after logging in
    /// </summary>
    public string RelativeLoginUrl { get; internal set; }

    /// <summary>
    /// Relative path to an inteligenz logo, should be a path
    /// that requires no authentication
    /// </summary>
    public string RelativeMicroceliumLogoPath { get; internal set; }
  }
}