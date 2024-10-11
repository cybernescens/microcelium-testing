using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <inheritdoc />
public abstract class WebComponent : IWebComponent
{
  /// <summary>
  ///   Initializes a <see cref="WebComponent" />
  /// </summary>
  /// <param name="driver">the <see cref="IWebDriverExtensions" /> Selenium Adapter</param>
  /// <param name="parent">the parent <see cref="IWebComponent" /></param>
  protected WebComponent(IWebDriverExtensions driver, IWebComponent? parent)
  {
    Driver = driver;
    Parent = parent;
  }

  /// <inheritdoc />
  public IWebDriverExtensions Driver { get; }

  /// <inheritdoc />
  public virtual IWebComponent? Parent { get; }

  /// <inheritdoc />
  public virtual void Wait() => Driver.WaitForElementToBeVisible(ElementIdentifier);

  /// <inheritdoc />
  public virtual By ElementIdentifier { get; } = By.CssSelector("*:first-child");

  /// <inheritdoc />
  public virtual IWebElement WebElement => Parent!.WebElement.FindElement(ElementIdentifier);

  /// <summary>
  /// Fired when the component is freshly loadedW
  /// </summary>
  protected internal event EventHandler? OnInitialized;

  /// <summary>
  /// Initializes the component
  /// </summary>
  /// <param name="sender">the initializing object</param>
  protected internal void Initialize(object? sender)
  {
    OnInitialized?.Invoke(sender, EventArgs.Empty);
  }

  protected virtual ISearchContext SearchContext => WebElement;

  /// <summary>
  /// Will find a child with the give <paramref name="selector"/> and expects both
  /// <see cref="Parent"/> and <see cref="WebElement"/> to no be null
  /// </summary>
  /// <param name="selector">the selector execute</param>
  /// <returns></returns>
  protected internal IWebElement? FindChild(By selector) => SearchContext.FindElement(selector);

  protected internal IWebElement? FindChild(string selector) => FindChild(By.CssSelector(selector));
  //protected internal T? FindChild<T>(string selector) where T : WebElement => (T?)FindChild(By.CssSelector(selector));

  /// <summary>
  /// Will find children  with the give <paramref name="selector"/> and expects both
  /// <see cref="Parent"/> and <see cref="WebElement"/> to no be null
  /// </summary>
  /// <param name="selector">the selector execute</param>
  /// <returns></returns>
  protected internal ReadOnlyCollection<IWebElement> FindChildren(By selector) =>
    SearchContext.FindElements(selector) ?? new ReadOnlyCollection<IWebElement>(Array.Empty<IWebElement>());
}

/// <inheritdoc />
public abstract class WebComponent<TParent> : WebComponent where TParent : IWebComponent
{
  /// <inheritdoc />
  protected WebComponent(IWebDriverExtensions driver, TParent? parent) : base(driver, parent) { }

  /// <summary>
  /// The base <see cref="WebComponent.Parent"/> returned as <typeparamref name="TParent"/>
  /// </summary>
  public TParent ParentComponent => (TParent)Parent!;
}