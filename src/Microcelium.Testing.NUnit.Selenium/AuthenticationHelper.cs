using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Selenium;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Cookie = System.Net.Cookie;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <inheritdoc />
  public class AuthenticationHelper : IAuthenticationHelper
  {
    private readonly ILogger<AuthenticationHelper> _log;

    /// <summary>
    /// Instantiates an <see cref="AuthenticationHelper"/>
    /// </summary>
    /// <param name="lf">the <see cref="ILoggerFactory"/></param>
    public AuthenticationHelper(ILoggerFactory lf) { _log = lf.CreateLogger<AuthenticationHelper>(); }

    /// <inheritdoc />
    public Task<CookieContainer> PerformAuth(IWebDriver drv, WebDriverConfig cfg)
    {
      var host = cfg.GetBaseUrl();
      var authority = cfg.GetAzureClientAuthority();
      
      _log.LogInformation($"Navigating to configured BaseUrl: `{host}`");

      drv.Navigate().GoToUrl(host);
      var redirected = new Uri(drv.Url);

      if (redirected.Host.Equals(authority.Host))
      {
        _log.LogInformation($"Have been redirected for login to `{redirected}`");
        drv.FindElement(By.CssSelector("input[type=\"email\"]")).SendKeys(cfg.Username);
        drv.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
        drv.FindElement(By.CssSelector("input[type=\"password\"]")).SendKeys(cfg.Password);
        drv.FindElement(By.CssSelector("input[value=\"Sign in\"]")).Click();
      }

      _log.LogInformation($"Logged in and waiting for confiured element by CSS Selector: `{cfg.LoggedInValidationSelector}`");
      drv.FindElement(By.CssSelector(cfg.LoggedInValidationSelector));

      var authCookies = new CookieContainer();
      var cc = new CookieCollection();
      drv.Manage().Cookies.AllCookies
        .Where(x => x.Domain.Contains(host.Host, StringComparison.InvariantCultureIgnoreCase))
        .Select(
          x => {
            var cookie = new Cookie {
              Domain = x.Domain,
              Name = x.Name,
              Value = x.Value,
              Path = x.Path
            };

            if (x.Expiry.HasValue)
              cookie.Expires = x.Expiry.Value;

            return cookie;
          })
        .ToList()
        .ForEach(x => cc.Add(x));

      var cookieNames = cc.Select(x => x.Name).Aggregate((acc, x) => $"{acc}\r\n\t{x}");
      _log.LogInformation($"Returning all cooke information in total there are `{cc.Count}` cookies.");
      _log.LogInformation($"Cookie Names include: \r\n\t{cookieNames}");

      authCookies.Add(cc);
      return Task.FromResult(authCookies);
    }
  }
}