using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public abstract class WebSite : WebComponent
{
  private readonly ILogger log;

  /* this is always going to be concrete types, i.e. types that implement Page<TPage> */
  private HashSet<WebPage> pages = new();

  protected WebSite(IWebDriverExtensions driver, IEnumerable<WebPage> pages) : base(driver, null)
  {
    this.pages = new HashSet<WebPage>(pages, WebPage.DefaultComparer);
    log = driver.LoggerFactory.CreateLogger<WebSite>();

    OnInitialized += (site, _) => {
      CurrentPage?.Initialize(site);
    };
  }

  /// <summary>
  /// The <see cref="WebPage"/> currently in the browser
  /// </summary>
  public WebPage CurrentPage { get; protected set; }

  /// <inheritdoc />
  public override By ElementIdentifier => By.CssSelector("html");

  /// <inheritdoc />
  protected override ISearchContext SearchContext => Driver;

  /// <summary>
  /// Attempts to load <typeparamref name="TPage"/> WebPage into the browser
  /// </summary>
  /// <typeparam name="TPage">the <see cref="WebPage"/> to load</typeparam>
  /// <param name="query"></param>
  /// <returns></returns>
  public TPage NavigateToPage<TPage>(object? queryString = null)
    where TPage : Page<TPage>
  {
    var page = Find<TPage>();
    if (page == null)
    {
      log.LogWarning("{PageType} not found in page cache", typeof(TPage));
      return (TPage)CurrentPage;
    }

    CurrentPage = page;
    Initialize(this);
    return (TPage)CurrentPage;
  }
  
  /// <summary>
  /// Attempts to load the WebPage of <paramref name="pageType"/> into the browser
  /// </summary>
  /// <param name="pageType">the <see cref="WebPage"/> to load</param>
  /// <param name="query"></param>
  /// <returns></returns>
  public WebPage NavigateToPage(Type pageType, object? query = null)
  {
    var hashed = new HashedPage(Driver, pageType);
    pages.TryGetValue(hashed, out var page);

    if (page == null)
    {
      log.LogWarning("{PageType} not found in page cache", pageType);
      return CurrentPage;
    }

    CurrentPage = page;
    Initialize(this);
    return CurrentPage;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="TPage"></typeparam>
  /// <returns></returns>
  protected TPage? Find<TPage>() where TPage : Page<TPage>
  {
    var hashed = new HashedPage(Driver, typeof(TPage));
    pages.TryGetValue(hashed, out var page);
    return (TPage?)page;
  }

  private class HashedPage : WebPage
  {
    public HashedPage(IWebDriverExtensions driver, Type pageType) : base(driver, pageType) { }
    public override int GetHashCode() => DefaultComparer.GetHashCode(this);
  }
}

public class Landing<TStartPage> : WebSite where TStartPage : Page<TStartPage>
{
  private readonly TStartPage landingPage;
  private readonly string baseAddress;
  private readonly string relativePath;
  private bool initialized;

  public Landing(IWebDriverExtensions driver, IEnumerable<WebPage> pages) : base(driver, pages)
  {
    baseAddress = driver.Config.BaseUri;

    var attr = typeof(TStartPage).GetCustomAttribute<RelativePathAttribute>();
    if (attr == null)
      throw new InvalidOperationException($"{typeof(TStartPage).Name} requires {nameof(RelativePathAttribute)}");

    relativePath = attr.Path;

    landingPage = Find<TStartPage>() ??
      throw new ArgumentException(
        nameof(TStartPage),
        $"Unable to find Landing Page `{typeof(TStartPage).FullName}` in site's pages.");

    CurrentPage = landingPage;
  }

  /// <summary>
  /// Called when the browser is first open and the page is navigated to.
  /// DOES NOT mean the site is finished loading, only that it is just starting.
  /// Initializes the a <see cref="WebSite"/> to the <see cref="WebPage"/>
  /// specified by <typeparamref name="TStartPage"/>
  /// </summary>
  /// <returns></returns>
  public WebPage Initialize()
  {
    Driver.Navigate().GoToUrl(Driver.Config.BaseUri + relativePath);
    initialized = true;
    Initialize(this);
    CurrentPage.Wait();
    return CurrentPage;
  }

  /// <summary>
  /// The <see cref="WebPage" /> first landed on when navigating to a <see cref="WebSite"/>
  /// </summary>
  public TStartPage Home =>
    initialized ? landingPage : throw new InvalidOperationException("Site has not been initialized");

  public override string ToString() => initialized ? $"{baseAddress} | {Home} [Landing]" : "Pending Initialization";
}