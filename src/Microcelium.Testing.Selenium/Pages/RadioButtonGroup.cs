using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

public abstract class RadioButtonGroup<TParent> : WebComponent<TParent> where TParent : WebComponent
{
  private readonly List<OptionBox<RadioButtonGroup<TParent>>> options = new();
  private readonly By optionSelector;

  protected RadioButtonGroup(IWebDriverExtensions driver, TParent parent, By optionSelector)
    : base(driver, parent)
  {
    this.optionSelector = optionSelector;

    OnInitialized += (_, _) => {
      var elements = Parent!.WebElement.FindElements(optionSelector);

      foreach (var input in elements)
      {
        var label = input.FindElement(By.XPath("//../label"));
        options.Add(
          new OptionBox<RadioButtonGroup<TParent>>(
            Driver,
            this,
            new OptionBox(input, label)));
      }
    };
  }

  public override By ElementIdentifier => optionSelector;
  protected override ISearchContext SearchContext => Parent!.WebElement;
  public override IWebElement WebElement => Parent!.WebElement;

  /// <summary>
  ///   The collection of Options for the button group
  /// </summary>
  public ReadOnlyCollection<OptionBox<RadioButtonGroup<TParent>>> Options => new(options);
}