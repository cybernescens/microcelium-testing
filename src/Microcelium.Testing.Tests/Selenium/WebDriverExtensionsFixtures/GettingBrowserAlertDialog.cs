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
  internal class GettingBrowserAlertDialog :
    IRequireLogger,
    IProvideServiceCollectionConfiguration,
    IRequireWebPage<AlertSite, AlertPage>
  {
    private string url;

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(AlertSite), typeof(AlertPage));
    }

    public AlertSite Site { get; set; }
    public AlertPage Page { get; set; }

    [Test]
    public async Task GetsAlertDialog()
    {
      using var webHost = WebHost.Start(
        url,
        router => router
          .MapGet(
            "/",
            (req, res, data) => {
              res.ContentType = "text/html; charset=utf-8";
              return res.WriteAsync(
                @"<html>
                  <body>
                  <div class='container'></div>
                  <script type='text/javascript'>
                    (function() {
                      setTimeout(function() {alert('Hello! I am an alert box!');}, 1000);                  
                    })();
                  </script>
                  </body>
                  </html>");
            }));

      Page.Navigate();
      await Page.Wait();
      Page.DismissAlert(true);
    }

    [Test]
    public async Task TimeoutIfDialogIsNotPresent()
    {
      using var webHost = WebHost.Start(
        url,
        router => router
          .MapGet(
            "/",
            (req, res, data) => {
              res.ContentType = "text/html; charset=utf-8";
              return res.WriteAsync(
                "<html><body><div class='container'></div><script type=\"text/javascript\"></script></body></html>");
            }));

      Action act = () => {
        Page.Navigate();
        Page.Wait().GetAwaiter().GetResult();
        Page.DismissAlert(false);
      };

      act.Should().Throw<WebDriverTimeoutException>();
    }
  }

  internal class AlertSite : WebSite
  {
    public AlertSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class AlertPage : WebPage<AlertPage>
  {
    private readonly ILogger<AlertPage> log;

    public AlertPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout)
    {
      log = lf.CreateLogger<AlertPage>();
    }

    public override By LoadedIdentifier => By.CssSelector("div.container");
    public override string RelativePath => "/";

    public void DismissAlert(bool ensure)
    {
      var alert = Parent.Driver.GetAlert(log);
      if (ensure)
        alert.Should().NotBeNull();

      alert.Dismiss();
    }
  }
}