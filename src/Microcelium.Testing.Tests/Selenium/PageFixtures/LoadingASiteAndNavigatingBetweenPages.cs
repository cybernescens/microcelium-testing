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

[RequireWebEndpoint]
[RequireSelenium]
internal class LoadingASiteAndNavigatingBetweenPages :
  IConfigureWebDriverConfig,
  IRequireWebHostOverride,
  IRequireScreenshots,
  IRequireWebSite<LoadingASiteAndNavigatingBetweenPages.Page1>
{
  public IHost Host { get; set; }
  public IWebDriverExtensions Driver { get; set; }
  public Uri HostUri { get; set; }
  public Landing<Page1> Site { get; set; }

  public void Configure(WebDriverConfig config) { config.BaseUri = HostUri.ToString(); }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet(
      "/page1",
      context => {
        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync(
          "<html><body><a href=\"page2\">Page 2</a></body></html>");
      });

    endpoint.MapGet(
      "/page2",
      context => {
        context.Response.ContentType = "text/html";
        return context.Response.WriteAsync(
          "<html><body><label><input type=\"radio\" name=\"foo-bar\">Foo</label><label><input type=\"radio\" name=\"foo-bar\">Bar</label></body></html>");
      });
  }

  [Test, Order(0)]
  public void Page1_Is_Link_To_Page2()
  {
    Site.Home.Page2Link.Should().BeEquivalentTo(new { Text = "Page 2", TagName = "a" });
    Site.Home.ClickLinkToPage2().Should().BeOfType<Page2>();
  }

  [Test, Order(1)]
  public void NavigateToPage1ThenPage2AndClickTheRadioButtonUsingType()
  {
    Site.CurrentPage.Should().BeOfType<Page2>();
    ((Page2)Site.CurrentPage)
      .RadioButton
      .Click()
      .Should()
      .BeEquivalentTo(
        new {
          LabelText = "Foo",
          IsSelected = true
        });
  }

  [RelativePath("/page1")]
  internal class Page1 : Page<Page1>
  {
    public Page1(IWebDriverExtensions driver) : base(driver) { }

    public IWebElement Page2Link => FindChild("a");

    public Page2 ClickLinkToPage2()
    {
      Page2Link.Click();
      return Site.NavigateToPage<Page2>();
    }

    public Page2 ClickLinkToPage2TheHardWay()
    {
      Page2Link.Click();
      return (Page2)Site.NavigateToPage(typeof(Page2));
    }
  }

  [RelativePath("/page2")]
  internal class Page2 : Page<Page2>
  {
    public Page2(IWebDriverExtensions driver) : base(driver)
    {
      OnInitialized += (_, _) => {
        RadioButton = new RadioGroup(driver, this);
        RadioButton.Initialize(this);
      };
    }

    public RadioGroup RadioButton { get; private set; }

    internal class RadioGroup : RadioButtonGroup<Page2>
    {
      public RadioGroup(IWebDriverExtensions driver, Page2 parent) : base(driver, parent, By.CssSelector("input")) { }

      public OptionBox<RadioButtonGroup<Page2>> Click()
      {
        Options[0].Click();
        return Options[0];
      }
    }
  }
}