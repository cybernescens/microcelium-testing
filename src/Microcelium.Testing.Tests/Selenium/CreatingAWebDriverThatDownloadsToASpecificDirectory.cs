using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.Fixtures)]
  [EnsureCleanDownloadDirectory(@"Downloads", true, ActionTargets.Test)]
  internal class CreatingAWebDriverThatDownloadsToASpecificDirectory : IRequireDownloadDirectory, IRequireLogger
  {
    public string DownloadDirectory { get; set; }

    [Test]
    public void SavesFileToDirectory()
    {
      var log = this.CreateLogger();
      var url = $"http://localhost:{TcpPort.NextFreePort()}";
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      args.Add("BaseUrl", url);
      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

      using (WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
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
            })))
      using (var driver = WebDriverFactory.Create(browserConfig, DownloadDirectory))
      {
        driver.Navigate().GoToUrl(url);
        driver.FindElement(By.LinkText("download")).Click();
        var fileInfo = driver.WaitForFileDownload(
          DownloadDirectory,
          "download.txt",
          log,
          TimeSpan.FromSeconds(10));

        fileInfo.Exists.Should()
          .BeTrue("file '{0}' should exist", fileInfo.FullName);
      }
    }
  }
}