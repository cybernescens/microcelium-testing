using System;
using System.Collections.ObjectModel;
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

      pageLoadTask = Task
        .Run(
          () => {
            OnComponentLoading?.Invoke(this, new ComponentLoadEvent { Page = this, Path = path });
            Parent.Driver.Navigate().GoToUrl(path);
            Parent.Driver.FindElement(LoadedIdentifier);
            Parent.Driver.DefinitivelyWaitForAnyAjax(log, Timeout);
            pageLoaded = true;
            OnComponentLoaded?.Invoke(this, new ComponentLoadEvent { Page = this, Path = path });
          },
          CancellationToken.None);

      return this;
    }

    /// <inheritdoc />
    public IWebSite Parent { get; internal set; }

    /// <inheritdoc />
    public Task Wait()
    {
      if (pageLoadTask == null)
        throw new InvalidOperationException(
          "The page must be navigated to at least once in its lifecycle to be Waited on."); 

      return pageLoadTask;
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