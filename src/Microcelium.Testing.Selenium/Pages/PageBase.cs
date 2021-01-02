using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  /// Base object for a page that can be navigated to
  /// </summary>
  public abstract class PageBase : IWebPage
  {
    protected ILogger log;

    /// <summary>
    /// the <see cref="IWebDriver"/>
    /// </summary>
    public IWebDriver Driver { get; private set; }

    /// <summary>
    /// The configuration object
    /// </summary>
    public IWebDriverConfig Config { get; private set; }

    /// <summary>
    /// if desired, a the configured timeout for the page
    /// </summary>
    protected virtual TimeSpan PageTimeout => Config.PageLoadTimeout;

    /// <summary>
    /// Initializes the page
    /// </summary>
    /// <param name="driver"></param>
    /// <param name="config"></param>
    public virtual void Initialize(IWebDriver driver, IWebDriverConfig config, ILogger log)
    {
      if (Driver != null)
        throw new InvalidOperationException("Page has already been initialized");

      this.log = log;
      Config = config;
      Driver = driver;
      Driver.DefinitivelyWaitForAnyAjax(log, PageTimeout);
    }

    /// <summary>
    /// Creates a &quot;link&quot; to a different page
    /// </summary>
    /// <typeparam name="TPage">the type of <see cref="Page"/></typeparam>
    /// <returns></returns>
    public TPage CreatePage<TPage>() where TPage : IWebPage, new()
    {
      var page = new TPage();
      page.Initialize(Driver, Config, log);
      return page;
    }
  }

  /// <summary>
  /// A &quot;strongly&quot; typed version of the page
  /// </summary>
  public abstract class Page<TPage> : Page where TPage : Page<TPage>
  {
    /// <summary>
    /// Once the page is loaded a reference to self
    /// </summary>
    /// <returns></returns>
    public TPage PageShouldBeLoaded()
    {
      WaitForPageToLoad();
      return (TPage)this;
    }
  }

  /// <summary>
  /// Represents a page one can navigate to
  /// </summary>
  public abstract class Page : PageBase, IHaveRelativePath
  {
    private readonly TimeSpan? timeout;

    /// <inheritdoc />
    protected Page() { }

    /// <inheritdoc />
    protected Page(TimeSpan timeout)
    {
      this.timeout = timeout;
    }

    /// <inheritdoc />
    protected override TimeSpan PageTimeout => timeout ?? Config.PageLoadTimeout;

    /// <summary>
    /// Waits for the <see cref="PageLoadedIdentifier"/> to load before continuing
    /// </summary>
    public void WaitForPageToLoad() => Driver.WaitForElementToBeVisible(PageLoadedIdentifier);

    /// <summary>
    /// A unique Selector for the page
    /// </summary>
    protected abstract By PageLoadedIdentifier { get; }

    /// <summary>
    /// the relative path to this <see cref="Page"/>
    /// </summary>
    public abstract string RelativePath { get; }

    /// <summary>
    /// The page''s title
    /// </summary>
    public string Title => Driver.Title;
  }
}