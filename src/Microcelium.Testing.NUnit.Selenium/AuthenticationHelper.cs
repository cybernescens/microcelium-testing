using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Microcelium.Testing.AspNetCore.Handlers;
using Microcelium.Testing.Selenium;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.NUnit.Selenium
{
  public class AuthenticationHelper : IAuthenticationHelper
  {
    private readonly CookieContainer authCookies;

    private readonly IWebDriverConfig config;
    private readonly ILogger log;
    private bool initialized;

    public AuthenticationHelper(IWebDriverConfig config, ILogger log)
    {
      this.config = config;
      this.log = log;
      authCookies = new CookieContainer();
      initialized = false;
    }

    /// <inheritdoc />
    public CookieContainer AuthCookies => !initialized ? Initialize() : authCookies;

    private CookieContainer Initialize()
    {
      log.LogInformation($"Checking that environment `{config.BaseUrl}` is alive...");

      using (var handler = new HttpClientHandler {CookieContainer = authCookies, AllowAutoRedirect = false})
      using (var client = new HttpClient(new MicroceliumLoggingDelegatingHandler(handler, log)) {BaseAddress = config.BaseUrl})
      {
        Task.WaitAll(StartSessionAsync(client));
        Task.WaitAll(PerformAuth(client));
      }

      log.LogInformation("Environment check complete");
      initialized = true;
      return authCookies;
    }

    private async Task StartSessionAsync(HttpClient client)
    {
      log.LogInformation($"Checking endpoint '{0}'", client.BaseAddress);
      using (var result = await client.GetAsync("/").ConfigureAwait(false))
        result.StatusCode.Should().Be(HttpStatusCode.Redirect, "(Pre-login) Did not receive login redirect to SSO");
    }

    private async Task PerformAuth(HttpClient client)
    {
      var jwtToken = await GeJwtTokenAsync(client).ConfigureAwait(false);

      using (var writer = new StringWriterWithEncoding(Encoding.UTF8))
      using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings {Encoding = Encoding.UTF8}))
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
              new[]
                {
                  new KeyValuePair<string, string>("wresult", token),
                  new KeyValuePair<string, string>("wa", "wsignin1.0"),
                  new KeyValuePair<string, string>("wctx", "rm=0&id=passive&ru=%2f")
                }))
          .ConfigureAwait(false))
        {
          response.StatusCode.Should().Be(HttpStatusCode.Redirect, "(Post-login) Did not like our generated auth-cookie ");
          response.Headers.Location.Should().Be("/", "(Post-login) Did not receive original url root redirect");
        }

        using (var response = await client.GetAsync(config.RelativeLoginUrl).ConfigureAwait(false))
          response.EnsureSuccessStatusCode();
      }
    }

    private async Task<string> GeJwtTokenAsync(HttpClient client)
    {
      using (var response = await client.GetAsync("/").ConfigureAwait(false))
      {
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "(Pre-login) Should have received redirect to SSO");
        var context = response.Headers.Location.ParseQueryString().Get("microceliumctx");

        using (var tokenResponse = await client.PostAsync(
            response.Headers.Location,
            new FormUrlEncodedContent(
              new Dictionary<string, string>
                {
                  {"username", config.Username},
                  {"password", config.Password},
                  {"scope", context}
                }))
          .ConfigureAwait(false))
        {
          tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK, "(Handshake) Did not like credentials");
          var token = await tokenResponse.Content.ReadAsAsync<dynamic>().ConfigureAwait(false);
          return (string)token.access_token;
        }
      }
    }

    private class StringWriterWithEncoding : StringWriter
    {
      public StringWriterWithEncoding(Encoding encoding)
      {
        Encoding = encoding;
      }

      public override Encoding Encoding { get; }
    }
  }
}