using System.Net;
using System.Threading.Tasks;
using Microcelium.Testing.Selenium;
using OpenQA.Selenium;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Exactly what it sounds like
  /// </summary>
  public class NoOpAuthenticationHelper : IAuthenticationHelper
  {
    /// <inheritdoc />
    public Task<CookieContainer> PerformAuth(IWebDriver drv, WebDriverConfig cfg) =>
      Task.FromResult(new CookieContainer());
  }
}