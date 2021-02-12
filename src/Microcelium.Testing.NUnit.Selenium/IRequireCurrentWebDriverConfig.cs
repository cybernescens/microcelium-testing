using Microcelium.Testing.Selenium;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Test decorator for when access to the <see cref="WebDriverConfig"/> is necessary
  /// </summary>
  public interface IRequireCurrentWebDriverConfig
  {
    /// <summary>
    /// The <see cref="WebDriverConfig"/>
    /// </summary>
    WebDriverConfig Config { get; set; }
  }
}