using System.Collections.Specialized;
using System.Threading.Tasks;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.Fixtures)]
  class CreatingAWebDriverWithAnInitializationStep : IRequireLogger
  {
    private IWebDriver fakeWebDriver;
    private IWebDriverConfig fakeConfig;

    [SetUp]
    public void SetUp()
    {
      var log = this.CreateLogger();
      var args = new NameValueCollection();
      fakeWebDriver = Substitute.For<IWebDriver>();
      args.Add("selenium.baseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      args.Add("webdriver.browser.type", "fake-webdriver");
      fakeConfig = WebDriver.Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log).Build();
      WebDriverFactory.AddDriverBuilder("fake-webdriver", c => fakeWebDriver);
    }

    [TearDown]
    public void TearDown()
    {
      WebDriverFactory.DriverBuilders.Remove(fakeConfig.BrowserType);
    }

    [Test]
    public async Task BrowserIsAlwaysDisposed()
    {
      var (driver, exceptionInfo) = WebDriverFactory.CreateAndInitialize(fakeConfig, (cfg, x) => { });
      using (driver)
      {
        var i = 1;
      }

      fakeWebDriver.Received().Dispose();
    }
  }
}