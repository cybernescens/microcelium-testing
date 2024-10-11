using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public interface IWebSite
{
  WebPage CurrentPage { get; }
  TPage NavigateToPage<TPage>(string? query = null) where TPage : Page<TPage>, IHaveRelativePath;
  WebPage NavigateToPage(Type pageType, string? query = null);
  WebPage Load();
}

public abstract class WebSite : IWebSite
{
  /* this is always going to be concrete types, i.e. types that implement Page<TPage> */
  private readonly HashSet<WebPage> pages;
  private readonly ILogger log;

  protected WebSite(IWebDriverExtensions driver, IEnumerable<WebPage> pages)
  {
    Driver = driver;
    this.log = driver.LoggerFactory.CreateLogger<WebSite>();
    this.pages = new HashSet<WebPage>(pages.Select(x => x.SetSite(this)), WebPage.DefaultComparer);
  }

  public IWebDriverExtensions Driver { get; }
  public WebPage CurrentPage { get; protected set; }
  
  public TPage NavigateToPage<TPage>(string? queryString = null)
    where TPage : Page<TPage>, IHaveRelativePath
  {
    var page = Find<TPage>();
    if (page == null)
    {
      log.LogWarning("{PageType} not found in page cache", typeof(TPage));
      return (TPage)CurrentPage;
    }

    CurrentPage = page;
    return (TPage)CurrentPage;
  }

  public WebPage NavigateToPage(Type pageType, string? query = null)
  {
    var hashed = new HashedPage(Driver, pageType);
    pages.TryGetValue(hashed, out var page);

    if (page == null)
    {
      log.LogWarning("{PageType} not found in page cache", pageType);
      return CurrentPage;
    }

    CurrentPage = page;
    return CurrentPage;
  }

  public WebPage Load()
  {
    var relative = 
      CurrentPage.RelativePath.StartsWith("/", StringComparison.InvariantCultureIgnoreCase)
        ? CurrentPage.RelativePath
        : $"/{CurrentPage.RelativePath}";

    Driver.Navigate().GoToUrl(Driver.Config.BaseUri + relative);
    CurrentPage.WaitForPageToLoad();
    return CurrentPage;
  }

  protected TPage? Find<TPage>() where TPage : Page<TPage>
  {
    var hashed = new HashedPage(Driver, typeof(TPage));
    pages.TryGetValue(hashed, out var page);
    return (TPage?)page;
  }

  private class HashedPage : WebPage
  {
    public HashedPage(IWebDriverExtensions driver, Type pageType) : base(driver, pageType) { }
    protected override By PageLoadedIdentifier => throw new NotImplementedException();
    public override string RelativePath => throw new NotImplementedException();
    public override int GetHashCode() => DefaultComparer.GetHashCode(this);
  }
}

public class Landing<TStartPage> : WebSite where TStartPage : Page<TStartPage>
{
  private readonly TStartPage landingPage;
  private readonly string baseAddress;

  public Landing(IWebDriverExtensions driver, IEnumerable<WebPage> pages) : base(driver, pages)
  {
    baseAddress = driver.Config.BaseUri;
    landingPage = Find<TStartPage>() ??
      throw new ArgumentException(
        nameof(TStartPage),
        $"Unable to find Landing Page `{typeof(TStartPage).FullName}` in site's pages.");

    CurrentPage = landingPage;
  }

  public TStartPage Home => landingPage;

  public override string ToString() => $"{baseAddress} | {Home} [Landing]";
}