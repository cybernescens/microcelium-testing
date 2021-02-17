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
using Microcelium.Testing.NUnit.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.None)]
  internal class ImportingNetCookies : IRequireWebPage<CookieSite, CookiePage>, IProvideServiceCollectionConfiguration
  {
    private string url;

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(CookieSite), typeof(CookiePage), typeof(CheckCookiePage));
    }

    public CookieSite Site { get; set; }
    public CookiePage Page { get; set; }

    [Test]
    public async Task ImportsCookiesForDomain()
    {
      string actualCookieValue = null;

      var expectedCookieValue = "Bar";
      var cookieContainer = new CookieContainer();

      using var host = WebHost.Start(
        url,
        router => router
          .MapGet(
            "/",
            (req, res, data) => {
              res.Cookies.Append("Foo", expectedCookieValue);
              res.ContentType = "text/html";
              return res.WriteAsync("<html><body><div class='container'></div></body></html>");
            })
          .MapGet(
            "/checkcookie",
            (req, res, data) => {
              actualCookieValue = req.Cookies["Foo"];
              return res.WriteAsync("<html><body><div class='container'></div></body></html>");
            }));

      Page.Navigate();
      await Page.Wait();
      Site.Driver.ImportCookies(cookieContainer, new Uri(url));
      var check = Page.LinkToCheckCookie.Navigate();
      await check.Wait();
      actualCookieValue.Should().Be(expectedCookieValue);
    }
  }

  internal class CookieSite : WebSite
  {
    public CookieSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class CookiePage : WebPage<CookiePage>
  {
    public CookiePage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }
    public override By LoadedIdentifier => By.CssSelector(".container");
    public override string RelativePath => "/";
    public CheckCookiePage LinkToCheckCookie => Parent.PreparePage<CheckCookiePage>();
  }

  internal class CheckCookiePage : WebPage<CheckCookiePage>
  {
    public CheckCookiePage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }
    public override By LoadedIdentifier => By.CssSelector(".container");
    public override string RelativePath => "/checkcookie";
  }
}