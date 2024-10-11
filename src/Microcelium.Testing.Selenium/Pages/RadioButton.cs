using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public class RadioButton<TParent> : Checkbox<TParent> where TParent : IWebComponent
{
  public RadioButton(IWebDriver driver, TParent parent, IWebElement webElement, By findLabel)
    : base(driver, parent, webElement, findLabel) { }
}