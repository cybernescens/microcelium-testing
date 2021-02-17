using System.Net;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  ///   Assists in Authentication
  /// </summary>
  public interface IAuthenticationHelper
  {
    /// <summary>
    ///   Authorizes and additionally returns a <see cref="CookieContainer" /> with all loaded cookies
    /// </summary>
    /// <param name="drv">the <see cref="IWebDriver" /></param>
    /// <param name="cfg">the <see cref="WebDriverConfig" /></param>
    /// <returns></returns>
    Task<CookieContainer> PerformAuth(IWebDriver drv, WebDriverConfig cfg);
  }
}