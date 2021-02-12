using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microcelium.Testing.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.Fixtures)]
  class CreatingAWebDriverWithAnInitializationStep : IRequireLogger
  {
    private IWebDriver fakeWebDriver;
    private WebDriverConfig fakeConfig;

    [SetUp]
    public void SetUp()
    {
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      fakeWebDriver = Substitute.For<IWebDriver>();
      args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      args.Add("BrowserType", "fake-webdriver");
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      fakeConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;
      WebDriverFactory.AddDriverBuilder("fake-webdriver", (c, dd) => fakeWebDriver);
    }

    [TearDown]
    public void TearDown()
    {
      WebDriverFactory.DriverBuilders.Remove(fakeConfig.BrowserType);
    }

    [Test]
    public void BrowserIsAlwaysDisposed()
    {
      var (driver, __) = WebDriverFactory.CreateAndInitialize(fakeConfig, null, (_, __) => { });
      using (driver) { }

      fakeWebDriver.Received().Dispose();
    }
  }
}