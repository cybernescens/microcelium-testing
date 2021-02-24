using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  /// a child component of the <typeparamref name="TParent"/>
  /// </summary>
  /// <typeparam name="TParent"></typeparam>
  public interface IWebComponent<out TParent>
  {
    /// <summary>
    /// the components parent, either a <see cref="IWebSite"/> or <see cref="IWebPage"/>
    /// </summary>
    TParent Parent { get; }

    /// <summary>
    /// The <see cref="By"/> criteria executed by the <see cref="IWebDriver"/> to
    /// determine the page has completely loaded
    /// </summary>
    By LoadedIdentifier { get; }

    /// <summary>
    /// Asynchronously waits until the <see cref="LoadedIdentifier"/> element is found ,
    /// If a page load has not been initiated, it will throw an <see cref="InvalidOperationException"/>
    /// </summary>
    /// <returns></returns>
    Task Wait();

    /// <summary>
    /// Event fired just prior to the <see cref="IWebDriver"/> navigating to the page
    /// </summary>
    public event EventHandler<ComponentLoadEvent> OnComponentLoading;

    /// <summary>
    /// Event fired when the page is completely loaded and no pending ajax can be found.
    /// </summary>
    public event EventHandler<ComponentLoadEvent> OnComponentLoaded;
  }

  /// <summary>
  ///   a navigable child page of the <see cref="IWebSite"/>
  /// </summary>
  public interface IWebPage : IWebComponent<IWebSite>
  {
    /// <summary>
    /// The configured Timeout for the page, there is a configured default
    /// but it can also be overrode for each <see cref="IWebPage"/>
    /// </summary>
    TimeSpan Timeout { get; }

    /// <summary>
    /// The Relative Path to this resource
    /// </summary>
    string RelativePath { get; }

    /// <summary>
    /// The child <see cref="IPageComponent"/>s
    /// </summary>
    IPageComponent[] Components { get; }
  }

  /// <summary>
  /// a child component of a <see cref="IWebPage"/>
  /// </summary>
  public interface IPageComponent : IWebComponent<IWebPage> { }
}