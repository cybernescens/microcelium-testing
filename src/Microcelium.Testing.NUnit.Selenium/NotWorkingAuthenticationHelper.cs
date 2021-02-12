using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Microcelium.Testing.AspNetCore.Handlers;
using Microcelium.Testing.Selenium;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Cookie = OpenQA.Selenium.Cookie;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <inheritdoc />
  public class NotWorkingAuthenticationHelper : IAuthenticationHelper
  {
    private readonly ILogger log;

    /// <summary>
    /// Instantiates an <see cref="AuthenticationHelper"/>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="lf"></param>
    public NotWorkingAuthenticationHelper(ILoggerFactory lf)
    {
      this.log = lf.CreateLogger<AuthenticationHelper>();
    }

    private async Task InternalPerformAuth(HttpClient client, IWebDriver drv, WebDriverConfig cfg)
    {
      log.LogInformation($"Checking endpoint '{0}'", client.BaseAddress);
      using (var result = await client.GetAsync("/").ConfigureAwait(false))
      {
        result.StatusCode.Should().Be(HttpStatusCode.Redirect, "(Pre-login) Did not receive login redirect to SSO");
      }

      var jwtToken = await GeJwtTokenAsync(client, cfg).ConfigureAwait(false);

      using (var writer = new StringWriterWithEncoding(Encoding.UTF8))
      using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
      {
        var bytes = Encoding.UTF8.GetBytes(jwtToken);
        xmlWriter.WriteStartElement(
          "wsse",
          "BinarySecurityToken",
          "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        xmlWriter.WriteAttributeString("ValueType", null, "urn:ietf:params:oauth:token-type:jwt");
        xmlWriter.WriteAttributeString(
          "EncodingType",
          null,
          "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

        xmlWriter.WriteBase64(bytes, 0, bytes.Length);
        xmlWriter.WriteEndElement();

        xmlWriter.Flush();
        writer.Flush();
        var token = Uri.EscapeDataString(writer.ToString());

        using (var response = await client.PostAsync(
            "/",
            new FormUrlEncodedContent(
              new[] {
                new KeyValuePair<string, string>("wresult", token),
                new KeyValuePair<string, string>("wa", "wsignin1.0"),
                new KeyValuePair<string, string>("wctx", "rm=0&id=passive&ru=%2f")
              }))
          .ConfigureAwait(false))
        {
          response.StatusCode.Should().Be(
            HttpStatusCode.Redirect,
            "(Post-login) Did not like our generated auth-cookie ");

          response.Headers.Location.Should().Be("/", "(Post-login) Did not receive original url root redirect");
        }

        using (var response = await client.GetAsync(cfg.RelativeLoginUrl).ConfigureAwait(false))
        {
          response.EnsureSuccessStatusCode();
        }
      }
    }

    private async Task<string> GeJwtTokenAsync(HttpClient client, WebDriverConfig cfg)
    {
      using (var response = await client.GetAsync("/").ConfigureAwait(false))
      {
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "(Pre-login) Should have received redirect to SSO");
        var qs = response.Headers.Location.ParseQueryString();

        using (var tokenResponse = await client.PostAsync(
            response.Headers.Location,
            new FormUrlEncodedContent(
              new Dictionary<string, string> {
                { "username", cfg.Username },
                { "password", cfg.GetPassword().ToString() },
                { "scope", qs["ctx"] }
              }))
          .ConfigureAwait(false))
        {
          tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK, "(Handshake) Did not like credentials");
          var token = await tokenResponse.Content.ReadAsAsync<dynamic>().ConfigureAwait(false);
          return (string) token.access_token;
        }
      }
    }

    private class StringWriterWithEncoding : StringWriter
    {
      public StringWriterWithEncoding(Encoding encoding) { Encoding = encoding; }

      public override Encoding Encoding { get; }
    }

    /// <inheritdoc />
    public async Task<CookieContainer> PerformAuth(IWebDriver drv, WebDriverConfig cfg)
    {
      drv.Manage().Cookies.DeleteAllCookies();
      var authCookies = new CookieContainer();
      var baseUrl = cfg.GetBaseUrl();
      var host = baseUrl.Host;

      using (var handler = new HttpClientHandler { CookieContainer = authCookies, AllowAutoRedirect = false })
      using (var logging = new MicroceliumLoggingDelegatingHandler(handler, log))
      using (var client = new HttpClient(logging) { BaseAddress = baseUrl })
      {
        await InternalPerformAuth(client, drv, cfg);
      }

      log.LogInformation("Environment check complete");

      void ApplyCookiesToWebDriverAndNavigate()
      {
        var cc = authCookies;
        drv.Manage().Cookies.DeleteAllCookies();
        cc.GetCookies(cfg.GetBaseUrl())
          .Select(
            c => {
              if (c.Domain.Contains("localhost"))
                c.Domain = null;

              return c;
            })
          .ToList()
          .ForEach(
            cookie => drv.Manage()
              .Cookies.AddCookie(new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null)));

        drv.Navigate().GoToUrl(cfg.GetBaseUrl());
      }

      drv.GoToRelativeUrl(cfg.GetBaseUrl() + cfg.RelativeLogoPath);
      ApplyCookiesToWebDriverAndNavigate();
      drv.WaitForElementToBeVisible(By.CssSelector(cfg.LoggedInValidationSelector));
      drv.DefinitivelyWaitForAnyAjax(log, cfg.PageLoadTimeout);


     
      return authCookies;
    }
  }
}