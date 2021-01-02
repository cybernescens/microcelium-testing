using System.Net;

namespace Microcelium.Testing.AspNetCore
{
  public interface IRequireCookieContainerAccess
  {
    CookieContainer Cookies { set; }
  }
}