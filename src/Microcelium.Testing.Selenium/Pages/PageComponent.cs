using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <summary>
/// Represents a component on a Page, so that <see cref="WebComponent{TParent}.Parent"/>
/// is strongly typed
/// </summary>
/// <typeparam name="TParent">the parent <see cref="Page{TPage}"/></typeparam>
public abstract class PageComponent<TParent> : WebComponent<TParent> where TParent : Page<TParent>, IWebComponent
{
  protected PageComponent(IWebDriverExtensions driver, TParent parent) : base(driver, parent) { }
}