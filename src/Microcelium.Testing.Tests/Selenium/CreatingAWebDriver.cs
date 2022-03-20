using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium;

[Parallelizable(ParallelScope.Fixtures)]
[RequireWebEndpoint]
[RequireSelenium]
internal class CreatingAWebDriver : IRequireSeleniumHost, IConfigureHostApplication, IConfigureWebHostAddress
{
  private string tempuri;

  public void Apply(HostBuilderContext context, IConfigurationBuilder builder)
  {
    var uri = GetHostUri();

    builder.AddInMemoryCollection(
      new KeyValuePair<string, string>[] {
        new("WebDriver:BaseUri", uri),
        new("WebDriver:Browser:Size:Width", "1024"),
        new("WebDriver:Browser:Size:Height", "768")
      });
  }

  public IHost Host { get; set; }

  [Test]
  public Task CreatesAChromeDriver()
  {
    Driver.DriverType.Should().BeAssignableTo<ChromeDriver>();
    return Task.CompletedTask;
  }

  [Test]
  public Task SetsBrowserSize()
  {
    Driver.Manage().Window.Size.Should().Be(new System.Drawing.Size(1024, 768));
    return Task.CompletedTask;
  }

  public IWebDriverExtensions Driver { get; set; }
  public WebApplication Endpoint { get; set; }
  public Uri HostUri { get; set; }

  public string GetHostUri()
  {
    if (string.IsNullOrEmpty(tempuri))
      tempuri = $"http://localhost:{TcpPort.NextFreePort()}";

    return tempuri;
  }
}