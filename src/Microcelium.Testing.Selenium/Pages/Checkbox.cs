using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public class Checkbox<TParent> : ComponentBase<TParent> where TParent : IWebComponent
{
  private readonly By findLabel;
  private readonly IWebElement webElement;

  public Checkbox(IWebDriver driver, TParent parent, IWebElement webElement, By findLabel)
    : base(driver, parent)
  {
    this.webElement = webElement;
    this.findLabel = findLabel;
  }

  public bool IsSelected => webElement.Selected;

  public string LabelText => webElement.FindElement(findLabel).Text;

  public Checkbox<TParent> Click()
  {
    webElement.Click();
    return this;
  }
}