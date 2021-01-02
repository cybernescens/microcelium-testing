using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  internal class CreatingAWebDriver : IRequireLogger
  {
    private ILogger log;

    [SetUp]
    public void Setup()
    {
      this.log = this.CreateLogger();
    }

    [Test]
    public async Task CreatesAChromeDriver()
    {
      var args = new NameValueCollection();
      args.Add("selenium.baseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      var browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      using (var driver = WebDriverFactory.Create(browserConfig))
      {
        driver.Should().BeOfType<ChromeDriver>();
      }
    }

    [Test]
    public async Task SetsBrowserSize()
    {
      var args = new NameValueCollection();
      args.Add("webdriver.browser.size", "1280,1024");
      args.Add("selenium.baseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      var browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      using (var driver = WebDriverFactory.Create(browserConfig))
      {
        driver.Manage().Window.Size.Should().Be(new Size(1280, 1024));
      }
    }

    [Test]
    public void ThrowsNotImplementedExceptionForUnknownDriver()
    {
      Action act = () =>
        {
          var args = new NameValueCollection();
          args.Add("selenium.baseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
          args.Add("webdriver.browser.type", "no-factory-method");

          var browserConfig = WebDriver
            .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
            .Build();
          var temp = WebDriverFactory.Create(browserConfig);
        };

      act
        .Should()
        .Throw<Exception>()
        .WithMessage("No browser configured for type 'no-factory-method'");
    }
  }
}