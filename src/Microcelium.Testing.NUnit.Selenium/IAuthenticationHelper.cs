using System.Net;

namespace Microcelium.Testing.NUnit.Selenium
{
  public interface IAuthenticationHelper
  {
    CookieContainer AuthCookies { get; }
  }
}