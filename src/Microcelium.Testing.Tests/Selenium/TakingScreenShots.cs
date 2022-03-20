using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium;

[Parallelizable(ParallelScope.Fixtures)]
[RequireScreenshotsDirectory]
[RequireGenericHost]
internal class TakingScreenShots : IRequireLogging, IRequireScreenshots
{
  [Test]
  public Task SavesScreenShot()
  {
    var config = new WebDriverConfig { BaseUri = "https://www.google.com" };
    var factory = new WebDriverFactory(config);

    using var inner = factory.Create(new RuntimeConfig { ScreenshotDirectory = ScreenshotDirectory });
    using var driver = new WebDriverAdapter(inner, config, LoggerFactory);
    driver.Navigate().GoToUrl("https://www.google.com");

    var imageName = Path.Combine(ScreenshotDirectory, $"{nameof(SavesScreenShot)}.png");
    driver.SaveScreenshotForEachTab(imageName);

    var screenShotFile = Directory.EnumerateFiles(ScreenshotDirectory).Select(x => new FileInfo(x)).FirstOrDefault();
    screenShotFile.Exists.Should()
      .BeTrue("file '{0}' should exist", ScreenshotDirectory);

    return Task.CompletedTask;
  }

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
  public string ScreenshotDirectory { get; set; }
}