using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium;

[RequireWebEndpoint]
[RequireSelenium]
internal class CreatingAWebDriverThatDownloadsToASpecificDirectory : 
  IRequireDownloadDirectory, 
  IConfigureWebDriverConfig,
  IRequireWebHostOverride
{
  public void Configure(WebDriverConfig config)
  {
    config.BaseUri = HostUri.ToString();
  }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet("/", context => {
      context.Response.ContentType = "text/html";
      return context.Response.WriteAsync("<html><body><a href='download'>download</a></body></html>");
    });

    endpoint.MapGet("/download", context => {
      context.Response.ContentType = "application/octet-stream";
      context.Response.Headers.Append("Content-Disposition", @"attachment; filename=""download.txt""");
      return context.Response.WriteAsync("file content");
    });
  }

  [Test]
  public Task SavesFileToDirectory()
  {
    Driver.Navigate().GoToUrl(HostUri);
    Driver.FindElement(By.CssSelector("a[href='download']")).Click();
    var fileInfo = Driver.WaitForFileDownload("download.txt");

    fileInfo.Should().NotBeNull();
    fileInfo!.Exists.Should().BeTrue("file '{0}' should exist", fileInfo.FullName);

    return Task.CompletedTask;
  }

  public IHost Host { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Uri HostUri { get; set; }
}