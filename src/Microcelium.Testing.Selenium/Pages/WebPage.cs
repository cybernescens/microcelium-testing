using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  ///   A Conceptual "Web Page" hosted by a "Web Site"
  /// </summary>
  public abstract class WebPage<TWebPage> : IWebPage where TWebPage : IWebPage
  {
    private bool pageLoaded;
    private Task pageLoadTask;
    private Task pageUnloadTask;
    private readonly ILogger<WebPage<TWebPage>> log;

    /// <summary>
    /// Instantiates a <see cref="WebPage{TWebPage}"/>
    /// </summary>
    /// <param name="site">the host <see cref="IWebSite"/></param>
    /// <param name="lf">the <see cref="ILoggerFactory"/></param>
    /// <param name="timeout">the page's implicit wait timeout</param>
    protected WebPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null)
    {
      Parent = site;
      Timeout = timeout ?? site.Config.PageLoadTimeout;
      log = lf.CreateLogger<WebPage<TWebPage>>();
    }

    /// <summary>
    /// Shortcut to the <see cref="IWebDriver"/>
    /// </summary>
    protected IWebDriver Driver => Parent.Driver;

    /// <summary>
    /// Navigates to this page
    /// </summary>
    /// <param name="query">optional query parameters</param>
    /// <returns>a reference to itself</returns>
    public TWebPage Navigate(string query = null) => (TWebPage) HandleNavigate(query);

    /// <summary>
    /// Navigates to this page
    /// </summary>
    /// <param name="query">optional query parameters</param>
    /// <returns>a reference to itself</returns>
    protected IWebPage HandleNavigate(string query = null)
    {
      var builder = new UriBuilder(Parent.Config.GetBaseUrl());
      builder.Path = 
        (RelativePath ?? string.Empty).StartsWith("/", StringComparison.InvariantCulture)
          ? RelativePath
          : "/" + RelativePath;

      builder.Query = query;
      var path = builder.Uri;

      // multiple sequential steps here:
      // 1) fire loading event
      // 2) prepare unload
      // 3) fire navigate
      // 4) fire wait unload and forget (async)
      // 5) fire wait load and forget (async)
      //    5a) no js running, no pending ajax
      //    5b) fire loaded event

      Task Unload(IWebElement e) =>
        !IsStale(e)
          ? Task.Delay(TimeSpan.FromMilliseconds(250))
            .ContinueWith((_, o) => Unload((IWebElement) o), e)
          : Task.CompletedTask;

      Task Load() =>
        Task
          .Run(
            () => {
              /* (5a) */
              Parent.Driver.FindElement(LoadedIdentifier);
              Parent.Driver.DefinitivelyWaitForAnyAjax(log, Timeout);
            })
          .ContinueWith(
            _ => {
              /* (5b) */
              pageLoaded = true;
              OnComponentLoaded?.Invoke(this, new ComponentLoadEvent { Page = this, Path = path });
            });

      /* (1) */
      OnComponentLoading?.Invoke(this, new ComponentLoadEvent { Page = this, Path = path });
      /* (2) */
      var unloadElement = Parent.Driver.FindElement(By.CssSelector("body"));
      /* (3) */
      Parent.Driver.Navigate().GoToUrl(path);
      /* (4) */
      pageUnloadTask = Unload(unloadElement);
      /* (5) */
      pageLoadTask = Load();

      return this;
    }

    [DebuggerNonUserCode]
    private bool IsStale(IWebElement element)
    {
      try
      {
        element.Click();
        return false;
      }
      catch (StaleElementReferenceException)
      {
        return true;
      }
    }

    /// <inheritdoc />
    public IWebSite Parent { get; internal set; }

    /// <inheritdoc />
    public Task Wait()
    {
      if (pageUnloadTask == null)
        throw new InvalidOperationException(
          "The page must be navigated away from at least once in its lifecycle for unload to be Waited on.");

      if (pageLoadTask == null)
        throw new InvalidOperationException(
          "The page must be navigated to at least once in its lifecycle to be Waited on."); 

      return Task.WhenAll(pageUnloadTask, pageLoadTask);
    }

    /// <inheritdoc />
    public event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <inheritdoc />
    public event EventHandler<ComponentLoadEvent> OnComponentLoaded;

    /// <inheritdoc />
    public abstract By LoadedIdentifier { get; }

    /// <inheritdoc />
    public TimeSpan Timeout { get; }

    /// <inheritdoc />
    public abstract string RelativePath { get; }

    /// <inheritdoc />
    public virtual IPageComponent[] Components { get; } = new IPageComponent[0];

    /// <summary>
    /// Attempts to find a Single <see cref="IWebElement"/> by a CSS Selector
    /// </summary>
    /// <param name="css">the CSS Selector</param>
    /// <returns></returns>
    public IWebElement ElementByCss(string css)
    {
      log.LogWarning($"PageLoaded: {pageLoaded}; Finding by: {css}");
      return Parent.Driver.FindElement(By.CssSelector(css));
    }

    /// <summary>
    /// Attempts to find Many <see cref="IWebElement"/>s by a CSS Selector
    /// </summary>
    /// <param name="css">the CSS Selector</param>
    /// <returns></returns>
    public ReadOnlyCollection<IWebElement> ElementsByCss(string css)
    {
      log.LogWarning($"PageLoaded: {pageLoaded}; Finding by: {css}");
      return Parent.Driver.FindElements(By.CssSelector(css));
    }
  }
}