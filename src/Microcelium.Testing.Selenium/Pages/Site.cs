using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  public class Site : IWebSite
  {
    private static readonly Type WebPageType = typeof(IWebPage);
    private static readonly Type HasRelativePathType = typeof(IHaveRelativePath);
    private Uri baseAddress;
    private WebDriverConfig config;
    private IWebDriver driver;
    private ILogger log;

    void IWebSite.Initialize(IWebDriver d, WebDriverConfig c, ILogger l)
    {
      if (driver != null)
        throw new InvalidOperationException("Site has already been initialized");

      driver = d;
      config = c;
      log = l;
      baseAddress = c.GetBaseUrl();
    }

    public TPage NavigateToPage<TPage>(string queryString = null)
      where TPage : IWebPage, IHaveRelativePath, new() =>
      (TPage) NavigateToPage(new TPage(), queryString);

    public IWebPage NavigateToPage(Type pageType, string queryString = null) =>
      !WebPageType.IsAssignableFrom(pageType)
        ? throw new InvalidOperationException(
          $"Page type '{pageType}' does not implement '{WebPageType}' or inherit from '{typeof(PageBase)}'")
        : !HasRelativePathType.IsAssignableFrom(pageType)
          ? throw new InvalidOperationException($"Page type '{pageType}' does not implement '{HasRelativePathType}'")
          : pageType.GetConstructor(new Type[0]) == null
            ? throw new InvalidOperationException($"Page type '{pageType}' has no default constructor")
            : NavigateToPage((IWebPage) Activator.CreateInstance(pageType), queryString);

    private IWebPage NavigateToPage(IWebPage page, string queryString = null)
    {
      driver.Navigate().GoToUrl(
        new Uri(
          baseAddress,
          $"{((IHaveRelativePath) page).RelativePath}{(queryString == null ? "" : $"?{queryString}")}"));

      page.Initialize(driver, config, log);
      return page;
    }
  }
}