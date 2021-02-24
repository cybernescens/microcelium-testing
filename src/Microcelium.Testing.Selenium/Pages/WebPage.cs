using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  ///   A Conceptual "Web Page" hosted by a "Web Site"
  /// </summary>
  public abstract class WebPage<TWebPage> : WebComponent<IWebSite>, IWebPage where TWebPage : IWebPage
  {
    private static readonly Regex LeadSlash = new Regex("^/", RegexOptions.Compiled);
    private readonly ILogger<WebPage<TWebPage>> log;
    private Task pageLoadTask;
    private Task pageUnloadTask;

    /// <summary>
    /// Instantiates a <see cref="WebPage{TWebPage}"/>
    /// </summary>
    /// <param name="site">the parent <see cref="IWebSite"/></param>
    /// <param name="lf">a <see cref="ILoggerFactory"/></param>
    /// <param name="timeout">the page load timeout, or the configured default when not provided</param>
    protected WebPage(IWebSite site, ILoggerFactory lf, TimeSpan? timeout = null) : base(site, lf)
    {
      log = lf.CreateLogger<WebPage<TWebPage>>();
      Timeout = timeout ?? site.Config.PageLoadTimeout;
    }

    /// <inheritdoc />
    protected override IWebDriver Driver => Parent.Driver;

    /// <summary>
    /// Navigates to this page
    /// </summary>
    /// <param name="query">optional query parameters</param>
    /// <returns>a reference to itself</returns>
    public TWebPage Navigate(string query = null) => (TWebPage)HandleNavigate(query);

    /// <summary>
    /// Navigates to this page
    /// </summary>
    /// <param name="query">optional query parameters</param>
    /// <returns>a reference to itself</returns>
    protected IWebPage HandleNavigate(string query = null)
    {
      var builder = new UriBuilder(Parent.Config.GetBaseUrl());
      builder.Query = query ?? string.Empty;
      builder.Path =
        $"/{LeadSlash.Replace(RelativePath ?? string.Empty, string.Empty)}";
     
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
            .ContinueWith((_, o) => Unload((IWebElement)o), e)
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsStale(IWebElement element)
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
    public override event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <inheritdoc />
    public override event EventHandler<ComponentLoadEvent> OnComponentLoaded;

    /// <inheritdoc />
    public override Task Wait()
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
    public TimeSpan Timeout { get; }

    /// <inheritdoc />
    public virtual IPageComponent[] Components { get; } = new IPageComponent[0];

    /// <inheritdoc />
    public abstract string RelativePath { get; }

  }
}