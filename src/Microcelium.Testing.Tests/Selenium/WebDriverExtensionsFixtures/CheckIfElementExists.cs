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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.None)]
  internal class CheckIfElementExists : IRequireWebPage<ElementSite, ElementPage>,
    IProvideServiceCollectionConfiguration
  {
    private string url;

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);

      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(ElementSite), typeof(ElementPage));
    }

    public ElementSite Site { get; set; }
    public ElementPage Page { get; set; }

    public IWebHost CreateHost()
    {
      return WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) => {
              res.ContentType = "text/html";
              return res.WriteAsync("<body><div class='container'><div id='Foo' /></div></body>");
            }));
    }

    [Test]
    public async Task ReturnsMatchingElement()
    {
      using var host = CreateHost();
      Page.Navigate();
      await Page.Wait();
      Page.SafeFooElement.Should().NotBeNull();
    }

    [Test]
    public async Task ReturnsFalseForNoMatchingElement()
    {
      using var host = CreateHost();
      Page.Navigate();
      await Page.Wait();
      Page.SafeBarElement.Should().BeNull();
    }
  }

  internal class ElementSite : WebSite
  {
    public ElementSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class ElementPage : WebPage<ElementPage>
  {
    public ElementPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }
    public override By LoadedIdentifier => By.CssSelector("div.container");
    public override string RelativePath => "/";

    public IWebElement SafeFooElement => Parent.Driver.ElementExists(By.Id("Foo"));
    public IWebElement SafeBarElement => Parent.Driver.ElementExists(By.Id("Bar"));
  }
}