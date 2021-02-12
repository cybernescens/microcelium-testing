using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium
{
  internal class TakingScreenShots : IRequireLogger
  {
    [Test]
    [EnsureCleanDirectory(@"Screenshots\TakingScreenShots", false)]
    public void SavesScreenShot()
    {
      var screenShotPath = @"Screenshots\TakingScreenShots\SavesScreenShot.png";
      var log = this.CreateLogger();
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BaseUrl", "https://www.google.com");
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

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