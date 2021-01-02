using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  [Parallelizable(ParallelScope.Fixtures)]
  [EnsureCleanDownloadDirectory(@"Downloads", true, ActionTargets.Test)]
  internal class CreatingAWebDriverThatDownloadsToASpecificDirectory : IRequireDownloadDirectory, IRequireLogger
  {
    public DirectoryInfo DownloadDirectory { get; set; }

    [Test]
    public async Task SavesFileToDirectory()
    {
      var log = this.CreateLogger();
      var url = $"http://localhost:{TcpPort.NextFreePort()}";
      var args = new NameValueCollection();
      args.Add("selenium.baseUrl", url);
      var browserConfig = WebDriver
        .Configure(cfg => 
          cfg.WithDefaultOptions().Providers(x => args[x]).DownloadDirectory(DownloadDirectory), log)
        .Build();

      using (WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) =>
              {
                res.ContentType = "text/html";
                return res.WriteAsync("<a href='download'>download</a>");
              })
          .MapGet(
            "download",
            (req, res, data) =>
              {
                res.ContentType = "application/octet-stream";
                res.Headers.Append("Content-Disposition", @"attachment; filename =""download.txt""");
                return res.WriteAsync("file content");
              })))
      using (var driver = WebDriverFactory.Create(browserConfig))
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