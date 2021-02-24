using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  ///   Conceptually a Page Component, i.e. a collection of related features on a page, or a mostly
  ///   autonomous component that relies on AJAX
  /// </summary>
  /// <typeparam name="TWebPage">the type of the component's parent</typeparam>
  public abstract class PageComponent<TWebPage> : WebComponent<TWebPage> where TWebPage : IWebPage
  {
    private static readonly Regex LeadSlash = new Regex("^/", RegexOptions.Compiled);
    private readonly ILogger<PageComponent<TWebPage>> log;

    /// <summary>
    ///   Instantiates a <see cref="PageComponent{TPageComponent}" />
    /// </summary>
    /// <param name="page">the parent <see cref="IWebPage" /></param>
    /// <param name="lf"></param>
    protected PageComponent(TWebPage page, ILoggerFactory lf) : base(page, lf)
    {
      this.log = lf.CreateLogger<PageComponent<TWebPage>>();
    }

    /// <summary>
    /// Shortcut to the <see cref="IWebDriver"/>
    /// </summary>
    protected override IWebDriver Driver => Parent.Parent.Driver;

    /// <summary>
    /// All <see cref="IWebElement"/>s that belong to this component are a child
    /// of this <see cref="IWebElement"/>
    /// </summary>
    public virtual IWebElement Container => Driver.FindElement(LoadedIdentifier);

    /// <summary>
    ///   Generally this will only be used when a component executes AJAX
    /// </summary>
    public override Task Wait()
    {
      var builder = new UriBuilder(Parent.Parent.Config.GetBaseUrl());
      builder.Path = $"/{LeadSlash.Replace(Parent.RelativePath ?? string.Empty, string.Empty)}#{GetType().Name}";

      return 
        Task
          .Run(
            () => {
              Driver.FindElement(LoadedIdentifier);
              Driver.DefinitivelyWaitForAnyAjax(log, Parent.Timeout);
            })
          .ContinueWith(
            _ => {
              OnComponentLoaded?.Invoke(this, new ComponentLoadEvent { Page = Parent, Path = builder.Uri });
            });
    }

    /// <inheritdoc />
    public override event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <inheritdoc />
    public override event EventHandler<ComponentLoadEvent> OnComponentLoaded;
  }
}
