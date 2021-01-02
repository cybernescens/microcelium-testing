using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  /// an Initialization and navigable page
  /// </summary>
  public interface IWebPage : IWebComponent
  {
    /// <summary>
    /// Initializes the page
    /// </summary>
    /// <param name="driver">the <see cref="IWebDriver"/></param>
    /// <param name="config"></param>
    /// <param name="log"></param>
    void Initialize(IWebDriver driver, IWebDriverConfig config, ILogger log);
  }
}