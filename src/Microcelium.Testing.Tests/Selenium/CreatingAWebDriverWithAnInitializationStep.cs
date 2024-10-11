using System.Threading.Tasks;
using Microcelium.Testing.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium;

[Parallelizable(ParallelScope.Fixtures)]
[RequireGenericHost]
internal class CreatingAWebDriverWithAnInitializationStep : IRequireLogging
{
  private static IWebDriver fakeWebDriver;

  /// <summary>
  /// The WebDriverConfig.Browser.DriverFactory only needs a public static
  /// method named &quot;Driver&quot; that takes two arguments: the <paramref name="config"/>
  /// and <paramref name="runtime"/> and returns a concrete <see cref="IWebDriver"/>
  /// </summary>
  /// <param name="config">the static <see cref="WebDriverConfig"/></param>
  /// <param name="runtime">the more dynamic <see cref="RuntimeConfig"/></param>
  /// <returns></returns>
  public static IWebDriver Driver(WebDriverConfig config, RuntimeConfig runtime)
  {
    fakeWebDriver = Substitute.For<IWebDriver>();
    return fakeWebDriver;
  }

  [Test]
  public Task BrowserIsAlwaysDisposed()
  {
    var configuration = new WebDriverConfig {
      BaseUri = $"http://localhost:{TcpPort.NextFreePort()}",
      Browser = new BrowserConfig {
        DriverFactory = typeof(CreatingAWebDriverWithAnInitializationStep).FullName!
      }
    };

    var wdf = new WebDriverFactory(configuration);
    var log = LoggerFactory.CreateLogger<CreatingAWebDriverWithAnInitializationStep>();

    using (var driver = wdf.Create(new RuntimeConfig()))
    {
      log.LogDebug("Created WebDriver {FakeDriver}", driver.GetType().FullName);
    }

    fakeWebDriver.Received().Dispose();
    return Task.CompletedTask;
  }

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}
