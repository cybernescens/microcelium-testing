using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  ///   A Conceptual "Web Page" hosted by a "Web Site"
  /// </summary>
  [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
  public abstract class WebComponent<TParent> : IWebComponent<TParent>
  {
    private readonly ILogger<WebComponent<TParent>> log;

    /// <summary>
    /// Instantiates a <see cref="WebPage{TWebPage}"/>
    /// </summary>
    /// <param name="site">the host <see cref="IWebSite"/></param>
    /// <param name="lf">the <see cref="ILoggerFactory"/></param>
    protected WebComponent(TParent site, ILoggerFactory lf)
    {
      Parent = site;
      log = lf.CreateLogger<WebComponent<TParent>>();
    }

    /// <summary>
    /// Shortcut to the <see cref="IWebDriver"/>
    /// </summary>
    protected abstract IWebDriver Driver { get; }

    /// <inheritdoc />
    public TParent Parent { get; /*internal set;*/ }

    /// <inheritdoc />
    public abstract Task Wait();
    
    /// <inheritdoc />
    public abstract event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <inheritdoc />
    public abstract event EventHandler<ComponentLoadEvent> OnComponentLoaded;

    /// <inheritdoc />
    public abstract By LoadedIdentifier { get; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IWebElement LoadedIdentifierWebElement => Driver.FindElement(LoadedIdentifier);

    /// <summary>
    /// Attempts to find a Single <see cref="IWebElement"/> by a CSS Selector
    /// </summary>
    /// <param name="css">the CSS Selector</param>
    /// <returns></returns>
    public IWebElement ElementByCss(string css) => Driver.FindElement(By.CssSelector(css));

    /// <summary>
    /// Attempts to find Many <see cref="IWebElement"/>s by a CSS Selector
    /// </summary>
    /// <param name="css">the CSS Selector</param>
    /// <returns></returns>
    public ReadOnlyCollection<IWebElement> ElementsByCss(string css) => Driver.FindElements(By.CssSelector(css));
  }
}