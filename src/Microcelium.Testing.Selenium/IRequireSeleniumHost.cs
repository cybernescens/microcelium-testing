namespace Microcelium.Testing.Selenium;

public interface IRequireSeleniumHost : IRequireHost
{
  /// <summary>
  ///   Initializes a WebDriver from defaults unless fixture also implements
  /// </summary>
  IWebDriverExtensions Driver { get; set; }
}