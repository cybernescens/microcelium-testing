using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public void CreatesAChromeDriver()
    {
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

      using var driver = WebDriverFactory.Create(browserConfig);
      driver.Should().BeOfType<ChromeDriver>();
    }

    [Test]
    public void SetsBrowserSize()
    {
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BrowserSize", "1280,1024");
      args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

      using var driver = WebDriverFactory.Create(browserConfig);
      driver.Manage().Window.Size.Should().Be(new Size(1280, 1024));
    }

    [Test]
    public void ThrowsNotImplementedExceptionForUnknownDriver()
    {
      Action act = () => {
        var services = new ServiceCollection();
        var args = new NameValueCollection();
        args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
        args.Add("BrowserType", "no-factory-method");
        services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
        var sp = services.BuildServiceProvider();
        var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;
        var temp = WebDriverFactory.Create(browserConfig);
      };

      act
        .Should()
        .Throw<Exception>()
        .WithMessage("No browser configured for type 'no-factory-method'");
    }
  }
}