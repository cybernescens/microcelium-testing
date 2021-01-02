using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.PageFixtures
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class LoadingASiteAndNavigatingBetweenPages : IRequireLogger
  {
    private IWebDriverConfig browserConfig;
    private Uri url;
    private IWebDriver webDriver;
    private IWebHost webHost;
    private ILogger log;

    [OneTimeSetUp]
    public async Task SetUp()
    {
      this.log = this.CreateLogger();
      var args = new NameValueCollection();
      url = new Uri($"http://localhost:{TcpPort.NextFreePort()}");
      args.Add("selenium.baseUrl", url.ToString());

      browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      webHost = WebHost.Start(
        url.ToString(),
        router => router
          .MapGet(
            "/page1",
            (req, res, data) =>
              {
                res.ContentType = "text/html";
                return res.WriteAsync("<body><a href='page2'>Page 2</a></body>");
              })
          .MapGet(
            "/page2",
            (req, res, data) =>
              {
                res.ContentType = "text/html";
                return res.WriteAsync("<body><label><input type='radio' />Foo</label></body>");
              }));
      webDriver = WebDriverFactory.Create(browserConfig);
    }

    [Test]
    public void NavigateToPage1ThenPage2AndClickTheRadioButtonUsingGenerics() =>
      webDriver
        .UsingSite<TestSite>(browserConfig, log)
        .NavigateToPage1()
        .ClickLinkToPage2()
        .GetRadioButton()
        .Click()
        .Should()
        .BeEquivalentTo(
          new
            {
              LabelText = "Foo",
              IsSelected = true
            });

    [Test]
    public void NavigateToPage1ThenPage2AndClickTheRadioButtonUsingType() =>
      ((Page1)webDriver
        .UsingSite<TestSite>(browserConfig, log)
        .NavigateToPage(typeof(Page1)))
      .ClickLinkToPage2()
      .GetRadioButton()
      .Click()
      .Should()
      .BeEquivalentTo(
        new
          {
            LabelText = "Foo",
            IsSelected = true
          });

    [OneTimeTearDown]
    public void TearDown()
    {
      SafelyTry.Dispose(webDriver);
      SafelyTry.Dispose(webHost);
    }

    private class TestSite : Site
    {
      public Page1 NavigateToPage1()
        => NavigateToPage<Page1>();
    }

    private class Page1 : PageBase, IHaveRelativePath
    {
      public string RelativePath => "page1";

      public Page2 ClickLinkToPage2()
      {
        Driver.FindElement(By.TagName("a")).Click();

        return CreatePage<Page2>();
      }
    }

    private class Page2 : PageBase
    {
      public RadioButton<Page2> GetRadioButton() =>
        new RadioButton<Page2>(
          Driver,
          this,
          Driver.FindElement(By.CssSelector("input[type='radio']")),
          By.XPath("./.."));
    }
  }
}