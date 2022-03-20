using System;
using System.Threading.Tasks;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures;

[Parallelizable(ParallelScope.Fixtures)]
[RequireScreenshotsDirectory]
[RequireWebEndpoint]
[RequireSelenium]
internal class WaitingForJavascriptResultToMatch : 
  IConfigureSeleniumWebDriverConfig,
  IRequireWebHostOverride, 
  IRequireScreenshots
{
  public void Configure(WebDriverConfig config)
  {
    config.BaseUri = HostUri.ToString();
    config.Timeout.Implicit = TimeSpan.FromSeconds(3);
  }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet(
      "/",
      context => {
        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync("<html><body></body></html>");
      });
  }

  [Test]
  public Task ReturnsTrueForExpectedMatch()
  {
    Driver.Navigate().GoToUrl(HostUri.ToString());
    Driver.WaitForJavascriptResult("6 / 3", 2);
    return Task.CompletedTask;
  }

  public IHost Host { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Uri HostUri { get; set; }
  public string ScreenshotDirectory { get; set; }
}