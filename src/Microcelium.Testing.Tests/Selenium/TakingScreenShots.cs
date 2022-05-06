using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium;

[RequireGenericHost]
internal class TakingScreenShots : IRequireLogging, IRequireScreenshots
{
  [Test]
  public Task SavesScreenShot()
  {
    var config = new WebDriverConfig { BaseUri = "https://www.google.com" };
    var factory = new WebDriverFactory(config);

    var dir = Path.Combine(
      TestContext.CurrentContext.TestDirectory,
      DateTime.Now.ToString("yyyyMMddHHmmss"),
      "Screenshots");

    if (Directory.Exists(dir))
      Directory.Delete(dir);

    Directory.CreateDirectory(dir);

    var runtime = new WebDriverRuntime { ScreenshotDirectory = dir };
    using var inner = factory.Create(runtime);
    using var driver = new WebDriverAdapter(inner, config, runtime, LoggerFactory);
    driver.Navigate().GoToUrl("https://www.google.com");

    driver.SaveScreenshotForEachTab($"{nameof(SavesScreenShot)}.png");

    var screenShotFile = Directory.EnumerateFiles(dir).Select(x => new FileInfo(x)).FirstOrDefault();
    screenShotFile.Exists.Should()
      .BeTrue("file '{0}' should exist", dir);

    return Task.CompletedTask;
  }

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}