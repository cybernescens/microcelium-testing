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
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class CheckIfElementExists : IRequireLogger
  {
    private string url;
    private IWebDriver webDriver;
    private IWebHost webHost;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      var log = this.CreateLogger();
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
                res.ContentType = "text/html";
                return res.WriteAsync("<body><div id='Foo' /></body>");
              }));
      webDriver = WebDriverFactory.Create(browserConfig);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      SafelyTry.Dispose(webDriver);
      SafelyTry.Dispose(webHost);
    }

    [Test]
    public void ReturnsMatchingElement()
    {
      webDriver.Navigate().GoToUrl(url);
      webDriver.ElementExists(By.Id("Foo")).Should().NotBeNull();
    }

    [Test]
    public void ReturnsFalseForNoMatchingElement()
    {
      webDriver.Navigate().GoToUrl(url);
      webDriver.ElementExists(By.Id("Bar")).Should().BeNull();
    }
  }
}