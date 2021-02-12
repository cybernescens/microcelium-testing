using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    public void SetUp()
    {
      var log = this.CreateLogger();
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);

      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

      webHost = WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) => {
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