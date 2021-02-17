using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microcelium.Testing.NUnit.Selenium;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.None)]
  internal class CreatingAWebDriverThatDownloadsToASpecificDirectory :
    IRequireWebPage<SelfHostedSite, DownloadPage>, 
    IRequireDownloadDirectory, 
    IRequireLogger,
    IProvideServiceCollectionConfiguration
  {
    private string url;

    public void Configure(IServiceCollection services)
    {
      url = $"http://localhost:{TcpPort.NextFreePort()}";
      var args = new NameValueCollection();
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      services.AddWebComponents(typeof(SelfHostedSite), typeof(DownloadPage));
    }

    public SelfHostedSite Site { get; set; }
    public DownloadPage Page { get; set; }

    [Test]
    public async Task SavesFileToDirectory()
    {
      var log = this.CreateLogger();
      var dd = this.GetDownloadDirectory();

      using var host = WebHost.Start(
        url,
        router => router
          .MapGet(
            "/",
            (req, res, data) => {
              res.ContentType = "text/html";
              return res.WriteAsync("<a href='download'>download</a>");
            })
          .MapGet(
            "download",
            (req, res, data) => {
              res.ContentType = "application/octet-stream";
              res.Headers.Append("Content-Disposition", @"attachment; filename =""download.txt""");
              return res.WriteAsync("file content");
            }));

      Page.Navigate();

      await Page.Wait();
      var fileInfo = Page.Download(dd);

      fileInfo.Exists.Should()
        .BeTrue("file '{0}' should exist", fileInfo.FullName);
    }
  }

  internal class SelfHostedSite : WebSite
  {
    public SelfHostedSite(IWebDriver driver, IOptions<WebDriverConfig> config) : base(driver, config) { }
  }

  internal class DownloadPage : WebPage<DownloadPage>
  {
    private readonly ILogger<DownloadPage> _log;

    public DownloadPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf, timeout)
    {
      _log = lf.CreateLogger<DownloadPage>();
    }

    public override By LoadedIdentifier => By.CssSelector("a[href=\"download\"]");
    public override string RelativePath => "/";

    public FileInfo Download(string dd)
    {
      Parent.Driver.FindElement(LoadedIdentifier).Click();
      return Parent.Driver.WaitForFileDownload(dd, "download.txt", _log);
    }
  }
}