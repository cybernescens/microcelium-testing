using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microcelium.Testing.NUnit.Selenium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.None)]
  internal class CreatingAWebDriver : IRequireLogger, IManageLogging
  {
    private ILogger log;

    [SetUp]
    public void Setup()
    {
      this.AddLogging();
      log = this.CreateLogger();
    }

    private static void ConfigureWebDriver(ServiceCollection services, NameValueCollection args)
    {
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.TryAddSingleton<IAuthenticationHelper, NoOpAuthenticationHelper>();
      services.TryAddSingleton(
        sp => {
          var auth = sp.GetRequiredService<IAuthenticationHelper>();
          var cfg = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;
          var dd = Path.GetTempPath();

          var (wd, initializationException) =
            WebDriverFactory.CreateAndInitialize(cfg, dd, (c, drv) => auth.PerformAuth(drv, c));

          initializationException?.Throw();
          return wd;
        }
      );
    }

    [Test]
    public void CreatesAChromeDriver()
    {
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");

      ConfigureWebDriver(services, args);

      var sp = services.BuildServiceProvider();
      using var driver = sp.GetRequiredService<IWebDriver>();
      driver.Should().BeOfType<ChromeDriver>();
      driver?.Dispose();
    }

    [Test]
    public void SetsBrowserSize()
    {
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BrowserSize", "1280,1024");
      args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");

      ConfigureWebDriver(services, args);

      var sp = services.BuildServiceProvider();
      using var driver = sp.GetRequiredService<IWebDriver>();
      driver.Manage().Window.Size.Should().Be(new Size(1280, 1024));
      driver?.Dispose();
    }

    [Test]
    public void ThrowsNotImplementedExceptionForUnknownDriver()
    {
      Action act = () => {
        var services = new ServiceCollection();
        var args = new NameValueCollection();
        args.Add("BaseUrl", $"http://localhost:{TcpPort.NextFreePort()}");
        args.Add("BrowserType", "no-factory-method");
        ConfigureWebDriver(services, args);
        var sp = services.BuildServiceProvider();
        using var driver = sp.GetRequiredService<IWebDriver>();
      };

      act
        .Should()
        .Throw<Exception>()
        .WithMessage("No browser configured for type 'no-factory-method'");
    }
  }
}