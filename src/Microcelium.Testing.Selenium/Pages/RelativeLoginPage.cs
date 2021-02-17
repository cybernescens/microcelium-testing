using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  /// A Page that represents the page found at <see cref="WebDriverConfig.RelativeLogoPath"/>
  /// </summary>
  public class RelativeLoginPage : WebPage<RelativeLoginPage>
  {
    /// <inheritdoc />
    public RelativeLoginPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }

    /// <inheritdoc />
    public override By LoadedIdentifier => By.CssSelector("a[href=\"/MicrosoftIdentity/Account/SignOut\"]");

    /// <inheritdoc />
    public override string RelativePath => Parent.Config.RelativeLoginUrl;
  }
}