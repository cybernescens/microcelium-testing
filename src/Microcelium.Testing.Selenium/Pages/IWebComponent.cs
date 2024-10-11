using System;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages;

/// <summary>
/// Those most basic component to encapsulate selenium. Has a reference to the Driver and to a Parent
/// </summary>
public interface IWebComponent
{
  /// <summary>
  /// The Selenium Driver Adapter
  /// </summary>
  IWebDriverExtensions Driver { get; }

  /// <summary>
  /// Parent Component. Only a <see cref="IWebSite"/> should have no Parent.
  /// </summary>
  IWebComponent? Parent { get; }

  /// <summary>
  /// The selector used to determine the component has fully loaded
  /// </summary>
  By ElementIdentifier { get; }

  /// <summary>
  /// Blocks until the component is loaded based on the <see cref="WebComponent.ElementIdentifier"/>
  /// </summary>
  void Wait();

  /// <summary>
  /// The underlying <see cref="IWebElement"/> for the component, defaults to the element
  /// returned by the <see cref="ElementIdentifier"/>
  /// </summary>
  IWebElement WebElement { get; }

  /// <summary>
  /// Fired when the component is freshly loaded
  /// </summary>
  //event EventHandler OnInitialized;
}