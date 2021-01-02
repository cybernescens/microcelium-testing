using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class GettingBrowserAlertDialog : IRequireLogger
  {
    private string url;
    private IWebDriver webDriver;
    private IWebHost webHost;
    private ILogger log;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      this.log = this.CreateLogger();
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("selenium.baseUrl", url);

      var browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      webHost = WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) =>
              {
                res.ContentType = "text/html; charset=utf-8";
                return res.WriteAsync(
                  "<html><body><script type=\"text/javascript\">alert('Hello! I am an alert box!');</script></body></html>");
              }));
      webDriver = WebDriverFactory.Create(browserConfig);
    }

    [Test]
    public void GetsAlertDialog()
    {
      webDriver.Navigate().GoToUrl(url);
      webDriver.GetAlert(log).Should().NotBeNull();
      webDriver.GetAlert(log).Dismiss();
    }

    [Test]
    public void TimesoutIfDialogIsNotPresent()
    {
      webDriver.Navigate().GoToUrl(url);
      webDriver.GetAlert(log).Dismiss();

      Action act = () => webDriver.GetAlert(log, TimeSpan.FromMilliseconds(100));

      act.Should().Throw<WebDriverTimeoutException>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      SafelyTry.Dispose(webDriver);
      SafelyTry.Dispose(webHost);
    }
  }
}