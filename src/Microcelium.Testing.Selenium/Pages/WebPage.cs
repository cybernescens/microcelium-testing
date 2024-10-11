using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <summary>
///   Represents a page one can navigate to
/// </summary>
public abstract class WebPage : IWebPage, IHaveRelativePath
{
  private readonly Type pageType;
  private IWebSite? site;

  protected WebPage(IWebDriverExtensions driver, Type pageType)
  {
    this.pageType = pageType ?? throw new ArgumentException(nameof(pageType));
    Driver = driver;
  }

  /// <summary>
  /// Parent sitew
  /// </summary>
  protected IWebSite Site
  {
    get => site ?? throw new InvalidOperationException($"`{nameof(Site)}` has not been initialized");
    set => site = value;
  }

  /// <summary>
  ///   the <see cref="IWebDriver" />
  /// </summary>
  public IWebDriverExtensions Driver { get; }

  /// <summary>
  ///   if desired, a the configured timeout for the page
  /// </summary>
  public virtual TimeSpan PageTimeout => Driver.Config.Timeout.PageLoad;

  /// <summary>
  ///   A unique Selector for the page
  /// </summary>
  protected abstract By PageLoadedIdentifier { get; }

  /// <summary>
  ///   The page''s title
  /// </summary>
  public string Title => Driver.Title;

  /// <summary>
  ///   the relative path to this <see cref="WebPage" />
  /// </summary>
  public abstract string RelativePath { get; }

  /// <summary>
  ///   Waits for the <see cref="PageLoadedIdentifier" /> to load before continuing
  /// </summary>
  public void WaitForPageToLoad() => Driver.WaitForElementToBeVisible(PageLoadedIdentifier);

  /// <inheritdoc />
  public override int GetHashCode() => DefaultComparer.GetHashCode(this);

  /// <inheritdoc />
  public override string ToString() => $"{pageType.Name} [{PageLoadedIdentifier}]";

  private sealed class WebPageComparer : IEqualityComparer<WebPage>
  {
    public bool Equals(WebPage? x, WebPage? y)
    {
      if (ReferenceEquals(x, y))
        return true;

      if (ReferenceEquals(x, null))
        return false;

      if (ReferenceEquals(y, null))
        return false;

      return x.pageType == y.pageType;
    }

    public int GetHashCode(WebPage obj) => obj.pageType.GetHashCode();
  }

  /// <summary>
  /// The Default Comparer to see if two pages are the same page.
  ///   Conceptually: has nothing to do with Content
  /// </summary>
  public static IEqualityComparer<WebPage> DefaultComparer { get; } = new WebPageComparer();

  internal WebPage SetSite(WebSite site)
  {
    Site = site;
    return this;
  }
}

/// <summary>
///   A &quot;strongly&quot; typed version of the page
/// </summary>
public abstract class Page<TPage> : WebPage where TPage : Page<TPage>
{
  protected Page(IWebDriverExtensions driver) : base(driver, typeof(TPage)) { }

  /// <summary>
  ///   Once the page is loaded a reference to self
  /// </summary>
  /// <returns></returns>
  public TPage PageShouldBeLoaded()
  {
    WaitForPageToLoad();
    return (TPage)this;
  }
}
