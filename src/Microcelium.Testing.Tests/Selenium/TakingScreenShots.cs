using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.NUnit;
using Microcelium.Testing.NUnit.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class TakingScreenShots : 
    IRequireWebPage<GoogleSite, GoogleHome>, 
    IRequireLogger, 
    IRequireScreenshots,
    IProvideServiceCollectionConfiguration
  {
    public GoogleSite Site { get; set; }
    public GoogleHome Page { get; set; }

    [Test]
    public async Task SavesScreenShot()
    {
      await Page.Navigate().Wait();

      var screenShotPath = this.GetScreenshotDirectory();
      var screenShotFile = new DirectoryInfo(screenShotPath);
      screenShotFile.Exists.Should()
        .BeTrue("file '{0}' should exist", screenShotPath);
    }

    public void Configure(IServiceCollection services)
    {
      var args = new NameValueCollection();
      args.Add("BaseUrl", "https://www.google.com");
      args.Add("RunHeadless", "false");
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(GoogleSite), typeof(GoogleHome));
    }
  }

  internal class GoogleSite : WebSite
  {
    public GoogleSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class GoogleHome : WebPage<GoogleHome>
  {
    public GoogleHome(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout) { }
    public override By LoadedIdentifier => By.CssSelector("input[value=\"Google Search\"]");
    public override string RelativePath => "/";
  }
}