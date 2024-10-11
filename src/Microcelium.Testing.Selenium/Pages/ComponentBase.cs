using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public abstract class ComponentBase<TParent> : IWebComponent where TParent : IWebComponent
{
  protected ComponentBase(IWebDriver driver, TParent parent)
  {
    Driver = driver;
    Parent = parent;
  }

  protected IWebDriver Driver { get; }
  protected TParent Parent { get; }
}

public abstract class PageComponent<TParent> : ComponentBase<TParent> where TParent : Page<TParent>
{
  protected PageComponent(IWebDriver driver, TParent parent) : base(driver, parent) { }
}