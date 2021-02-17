using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

namespace Microcelium.Testing.Selenium.PageFixtures
{
  [Parallelizable(ParallelScope.None)]
  internal class LoadingAPageAndNavigatingBetweenPages :
    IRequireWebPage<SiteTestsSite, Page1>,
    IRequireLogger,
    IProvideServiceCollectionConfiguration
  {
    private Uri url;

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      url = new Uri($"http://localhost:{TcpPort.NextFreePort()}");
      args.Add("BaseUrl", url.ToString());

      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(SiteTestsSite), typeof(Page1), typeof(Page2));
    }

    public SiteTestsSite Site { get; set; }
    public Page1 Page { get; set; }

    [Test]
    public async Task NavigateToPage1ThenPage2AndClickTheRadioButtonUsingType()
    {
      using var webHost = WebHost.Start(
        url.ToString(),
        router => router
          .MapGet(
            "/page1",
            (_, res, __) => {
              res.ContentType = "text/html";
              return res.WriteAsync("<body><a href='page2'>Page 2</a></body>");
            })
          .MapGet(
            "/page2",
            (_, res, __) => {
              res.ContentType = "text/html";
              return res.WriteAsync(
                @"<body>
                    <input type='radio' name='test' id='foo' value='foo' checked /><label for='foo'>Foo</label>
                    <input type='radio' name='test' id='bar' value='bar' /><label for='bar' class='bar'>Bar</label>
                  </body>");
            }));

      var page2 = Page.ClickLinkToPage2();
      page2.BarLabel.Click();
      page2.RadioButton.GetAttribute("value").Should().BeEquivalentTo("bar");
    }
  }

  internal class SiteTestsSite : WebSite
  {
    public SiteTestsSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class Page1 : WebPage<Page1>
  {
    public Page1(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }
    public override By LoadedIdentifier => By.CssSelector("a");
    public override string RelativePath => "page1";

    public Page2 ClickLinkToPage2()
    {
      var page = Parent.PreparePage<Page2>();
      page.Navigate();
      return page;
    }
  }

  internal class Page2 : WebPage<Page2>
  {
    private readonly Lazy<IWebElement> lazyRadio;

    public Page2(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout)
    {
      lazyRadio = new Lazy<IWebElement>(() => 
        ElementsByCss("input[type='radio']")
          .FirstOrDefault(x => x.Selected)
      );
    }

    public IWebElement BarLabel => ElementByCss("label.bar");
    public IWebElement RadioButton => lazyRadio.Value;
    public override By LoadedIdentifier => By.CssSelector("input");
    public override string RelativePath => "page2";
  }
}