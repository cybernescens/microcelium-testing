using System;
using FluentAssertions;
using Microcelium.Testing.Selenium.Pages;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.PageFixtures;

[Parallelizable(ParallelScope.Fixtures)]
[RequireScreenshotsDirectory]
[RequireWebEndpoint]
[RequireSelenium]
internal class LoadingASiteAndNavigatingBetweenPages :
  IConfigureSeleniumWebDriverConfig,
  IRequireWebHostOverride, 
  IRequireScreenshots,
  IRequireWebSite<LoadingASiteAndNavigatingBetweenPages.Page1>
{
  public IHost Host { get; set; }
  public Uri HostUri { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Landing<Page1> Site { get; set; }
  public string ScreenshotDirectory { get; set; }

  public void Configure(WebDriverConfig config)
  {
    config.BaseUri = HostUri.ToString();
  }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet(
      "/page1",
      context => {
        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync("<html><body><a href='page2'>Page 2</a></body></html>");
      });

    endpoint.MapGet(
      "/page2",
      context => {
        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync("<html><body><label><input type='radio' />Foo</label></body></html>");
      });
  }

  [Test]
  public void NavigateToPage1ThenPage2AndClickTheRadioButtonUsingGenerics() =>
    Site.Home
      .ClickLinkToPage2()
      .GetRadioButton()
      .Click()
      .Should()
      .BeEquivalentTo(
        new {
          LabelText = "Foo",
          IsSelected = true
        });

  [Test]
  public void NavigateToPage1ThenPage2AndClickTheRadioButtonUsingType() =>
    Site.Home
      .ClickLinkToPage2TheHardWay()
      .GetRadioButton()
      .Click()
      .Should()
      .BeEquivalentTo(
        new {
          LabelText = "Foo",
          IsSelected = true
        });

  internal class Page1 : Page<Page1>, IHaveRelativePath
  {
    public Page1(IWebDriverExtensions driver) : base(driver) { }
    protected override By PageLoadedIdentifier => By.CssSelector("a[href='page2']");
    public override string RelativePath => "/page1";

    public Page2 ClickLinkToPage2()
    {
      Driver.FindElement(By.TagName("a")).Click();
      return Site.NavigateToPage<Page2>();
    }

    public Page2 ClickLinkToPage2TheHardWay()
    {
      Driver.FindElement(By.TagName("a")).Click();
      return (Page2)Site.NavigateToPage(typeof(Page2));
    }
  }

  internal class Page2 : Page<Page2>
  {
    public Page2(IWebDriverExtensions driver) : base(driver) { }
    protected override By PageLoadedIdentifier => By.CssSelector("input[type='radio']");
    public override string RelativePath => "/page1";

    public RadioButton<Page2> GetRadioButton() =>
      new(
        Driver,
        this,
        Driver.FindElement(By.CssSelector("input[type='radio']")),
        By.XPath("./.."));
  }
}