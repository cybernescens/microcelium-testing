using System;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <summary>
/// Represents a Checkbox or Radiobutton
/// </summary>
/// <typeparam name="TParent">the parent <see cref="IWebComponent"/></typeparam>
public class OptionBox<TParent> : WebComponent<TParent> where TParent : WebComponent
{
  private readonly OptionBox innerbox;

  protected internal OptionBox(IWebDriverExtensions driver, TParent parent, By inputSelector, By? labelSelector = null) : base(driver, parent)
  {
    var input = parent.FindChild(inputSelector);

    if (input == null)
      throw new InvalidOperationException($"Unable to find child input with selector `{inputSelector}");

    var label = labelSelector != null ? parent.FindChild(labelSelector) : null;

    innerbox = new OptionBox(input, label);
  }

  protected internal OptionBox(IWebDriverExtensions driver, TParent parent, OptionBox optionBox) : base(driver, parent)
  {
    innerbox = optionBox;
  }

  protected override ISearchContext SearchContext => WebElement;

  /// <summary>
  /// Is the checkbox selected
  /// </summary>
  public bool IsSelected => innerbox.IsSelected;
  
  /// <summary>
  /// The label associate with the checkbox
  /// </summary>
  public string? LabelText => innerbox.LabelText;

  /// <summary>
  /// Click the checkbox
  /// </summary>
  /// <returns>reference to self</returns>
  public OptionBox<TParent> Click()
  {
    innerbox.Click();
    return this;
  }
}

/// <summary>
///   Represents a checkbox
/// </summary>
/// <typeparam name="TParent"></typeparam>
public class Checkbox<TParent> : OptionBox<TParent> where TParent : WebComponent
{
  public Checkbox(IWebDriverExtensions driver, TParent parent, By inputSelector, By? labelSelector = null) : 
    base(driver, parent, inputSelector, labelSelector) { }

  public Checkbox(IWebDriverExtensions driver, TParent parent, OptionBox optionBox) :
    base(driver, parent, optionBox) { }
}

/// <summary>
/// Represents a radio button
/// </summary>
/// <typeparam name="TParent"></typeparam>
public class RadioButton<TParent> : OptionBox<TParent> where TParent : WebComponent
{
  public RadioButton(IWebDriverExtensions driver, TParent parent, By inputSelector, By? labelSelector = null) : 
    base(driver, parent, inputSelector, labelSelector) { }

  public RadioButton(IWebDriverExtensions driver, TParent parent, OptionBox optionBox) :
    base(driver, parent, optionBox) { }
}

/// <summary>
///   Used internally by <see cref="OptionBox{TParent}" /> and <see cref="RadioButtonGroup{TParent}" />
/// </summary>
public class OptionBox
{
  private readonly IWebElement? label;
  private readonly IWebElement input;

  /* the Label selector as a child of the parent  */
  internal OptionBox(IWebElement input, IWebElement? label = null)
  {
    this.label = label;
    this.input = input;
  }

  public bool IsSelected => input.Selected;
  public string? LabelText => label?.Text;

  public void Click() { input.Click(); }
}