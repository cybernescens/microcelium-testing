using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <summary>
///   Represents a page one can navigate to
/// </summary>
public abstract class WebPage : WebComponent<WebSite>
{
  private readonly Type pageType;
  private WebSite? site;
  private bool initialized;
  private IWebElement webElement;

  protected WebPage(IWebDriverExtensions driver, Type pageType) : base(driver, null)
  {
    this.pageType = pageType ?? throw new ArgumentException(nameof(pageType));
    OnInitialized += (parentSite, _) => {
      site = (WebSite)parentSite!;
      webElement = site.FindChild(ElementIdentifier)!;
      initialized = true;
    };
  }

  /// <inheritdoc />
  public override IWebComponent Parent =>
    initialized ? site! : throw new InvalidOperationException($"Parent `{nameof(Site)}` has not initialized this page");

  /// <inheritdoc />
  public override By ElementIdentifier => By.CssSelector("body");

  /// <inheritdoc />
  public override IWebElement WebElement => 
    initialized ? webElement : throw new InvalidOperationException($"Parent `{nameof(Site)}` has not initialized this page");

  /// <summary>
  ///   Parent <see cref="WebSite" />
  /// </summary>
  protected WebSite Site =>
    initialized ? site! : throw new InvalidOperationException($"Parent `{nameof(Site)}` has not initialized this page");

  /// <summary>
  ///   if desired, a the configured timeout for the page
  /// </summary>
  public virtual TimeSpan PageTimeout => Driver.Config.Timeout.PageLoad;

  /// <summary>
  ///   The page's title
  /// </summary>
  public string Title => Driver.Title;

  /// <inheritdoc />
  public override int GetHashCode() => DefaultComparer.GetHashCode(this);

  /// <inheritdoc />
  public override string ToString() => $"{pageType.Name} [{ElementIdentifier}]";

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
  /// <returns>a reference to itself</returns>
  public TPage OnceLoaded()
  {
    Wait();
    return (TPage)this;
  }
}
