using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  ///   Conceptually a Page Component, i.e. a collection of related features on a page, or a mostly
  ///   autonomous component that relies on AJAX
  /// </summary>
  /// <typeparam name="TWebPage">the type of the component's parent</typeparam>
  public abstract class PageComponent<TWebPage> : IWebComponent<TWebPage>
    where TWebPage : IWebPage
  {
    /// <summary>
    ///   Instantiates a <see cref="PageComponent{TPageComponent}" />
    /// </summary>
    /// <param name="page">the parent <see cref="IWebPage" /></param>
    protected PageComponent(TWebPage page) { Parent = page; }

    /// <inheritdoc />
    public TWebPage Parent { get; }

    /// <summary>
    /// Shortcut to the <see cref="IWebDriver"/>
    /// </summary>
    protected IWebDriver Driver => Parent.Parent.Driver;

    /// <summary>
    /// All <see cref="IWebElement"/>s that belong to this component are a child
    /// of this <see cref="IWebElement"/>
    /// </summary>
    protected virtual IWebElement Container => Driver.FindElement(LoadedIdentifier);

    /// <summary>
    ///   Generally this will only be used when a component executes AJAX
    /// </summary>
    public virtual Task Wait() => Task.CompletedTask;

    /// <inheritdoc />
    public abstract By LoadedIdentifier { get; }

    /// <inheritdoc />
    public event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <inheritdoc />
    public event EventHandler<ComponentLoadEvent> OnComponentLoaded;
  }
}
