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

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.None)]
  internal class WaitingForJavascriptResultToMatch :
    IRequireLogger,
    IRequireWebPage<JavaScriptSite, JavaScriptPage>,
    IProvideServiceCollectionConfiguration
  {
    private string url;

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(JavaScriptSite), typeof(JavaScriptPage));
    }

    public JavaScriptSite Site { get; set; }
    public JavaScriptPage Page { get; set; }

    [Test]
    public async Task ReturnsTrueForExpectedMatch()
    {
      using var host = WebHost.Start(
        url,
        router => router
          .MapGet(
            "/",
            (req, res, data) => {
              res.ContentType = "text/html";
              return res.WriteAsync("<body></body>");
            }));

      Page.Navigate();
      await Page.Wait();
      Page.TestJavaScript();
    }
  }

  internal class JavaScriptSite : WebSite
  {
    public JavaScriptSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class JavaScriptPage : WebPage<JavaScriptPage>
  {
    private readonly ILogger<JavaScriptPage> log;

    public JavaScriptPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout)
    {
      log = lf.CreateLogger<JavaScriptPage>();
    }

    public override By LoadedIdentifier => By.CssSelector("body");
    public override string RelativePath => "/";

    public void TestJavaScript()
    {
      Parent.Driver.WaitForJavascriptResult("6 / 3", 2, log, TimeSpan.FromMilliseconds(100)).Should().Be(true);
    }
  }
}