using System;
using FluentAssertions;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures;

[Parallelizable(ParallelScope.Fixtures)]
[RequireScreenshotsDirectory]
[RequireWebEndpoint]
[RequireSelenium]
internal class CheckIfElementExists :  
  IConfigureSeleniumWebDriverConfig,
  IRequireWebHostOverride, 
  IRequireScreenshots
{
  [Test]
  public void ReturnsMatchingElement()
  {
    Driver.Navigate().GoToUrl(HostUri.ToString());
    Driver.ElementExists(By.Id("Foo")).Should().NotBeNull();
  }

  [Test]
  public void ReturnsFalseForNoMatchingElement()
  {
    Driver.Navigate().GoToUrl(HostUri.ToString());
    Driver.ElementExists(By.Id("Bar")).Should().BeNull();
  }

  public void Configure(WebDriverConfig config)
  {
    config.BaseUri = HostUri.ToString();
    config.Timeout.Implicit = TimeSpan.FromSeconds(1);
  }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet("/", context => {
      context.Response.ContentType = "text/html";
      return context.Response.WriteAsync("<html><body><div id='Foo' /></body></html>");
    });
  }

  public IHost Host { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Uri HostUri { get; set; }
  public string ScreenshotDirectory { get; set; }
}