using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.NUnit;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium
{
  internal class TakingScreenShots : IRequireLogger
  {
    [Test]
    [EnsureCleanDirectory(@"Screenshots\TakingScreenShots", false)]
    public async Task SavesScreenShot()
    {
      var screenShotPath = @"Screenshots\TakingScreenShots\SavesScreenShot.png";
      var log = this.CreateLogger();
      var args = new NameValueCollection();
      args.Add("selenium.baseUrl", "https://www.google.com");
      var browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      using (var driver = WebDriverFactory.Create(browserConfig))
      {
        driver.Navigate().GoToUrl("https://www.google.com");
        driver.SaveScreenshotForEachTab(screenShotPath, log);
      }

      var screenShotFile = new FileInfo(screenShotPath);
      screenShotFile.Exists.Should()
        .BeTrue("file '{0}' should exist", screenShotPath);
    }
  }
}