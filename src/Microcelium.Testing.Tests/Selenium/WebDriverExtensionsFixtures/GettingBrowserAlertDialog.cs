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
[RequiresScreenshotsDirectory]
[RequiresWebEndpoint]
[RequiresSelenium]
internal class GettingBrowserAlertDialog :  
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
        context.Response.ContentType = "text/html; charset=utf-8";
        return context.Response.WriteAsync(
          "<html><body><a href=\"javascript:alert('Hello! I am an alert box!');\" class=\"alert\">Alert</a></body></html>");
      });
  }

  [Test]
  public void GetsAlertDialog()
  {
    Driver.Navigate().GoToUrl(HostUri.ToString());
    Driver.FindElement(By.CssSelector(".alert")).Click();
    var alert = Driver.GetAlert();
    alert.Should().NotBeNull();
    alert!.Dismiss();
  }

  [Test]
  public void TimeoutIfDialogIsNotPresent()
  {
    Action act = () => {
      Driver.Navigate().GoToUrl(HostUri.ToString());
      Driver.GetAlert();
    };

    act.Should().Throw<WebDriverTimeoutException>();
  }

  public IHost Host { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Uri HostUri { get; set; }
  public string ScreenshotDirectory { get; set; }
}