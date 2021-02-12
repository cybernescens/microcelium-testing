using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium
{
  internal class ImportingNetCookies : IRequireLogger
  {
    [Test]
    public void ImportsCookiesForDomain()
    {
      string actualCookieValue = null;

      var expectedCookieValue = "Bar";
      var url = $"http://localhost:{TcpPort.NextFreePort()}";
      var cookieContainer = new CookieContainer();
      var log = this.CreateLogger();
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

      using var host = WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) => {
              res.Cookies.Append("Foo", expectedCookieValue);
              res.ContentType = "text/html";
              return res.WriteAsync("");
            })
          .MapGet(
            "checkcookie",
            (req, res, data) => {
              actualCookieValue = req.Cookies["Foo"];
              return res.WriteAsync("");
            }));

      using var cookieHandler = new HttpClientHandler {
        CookieContainer = cookieContainer,
        UseCookies = true,
        AllowAutoRedirect = false
      };

      using var httpClient = new HttpClient(cookieHandler) { BaseAddress = new Uri(url) };
      using var driver = WebDriverFactory.Create(browserConfig);
      driver.Navigate().GoToUrl(url);
      driver.ImportCookies(cookieContainer, new Uri(url));
      driver.Navigate().GoToUrl(url + "/checkcookie");
      actualCookieValue.Should().Be(expectedCookieValue);
    }
  }
}