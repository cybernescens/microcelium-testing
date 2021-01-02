using System.Web;
using System.Web.Mvc;

namespace Microcelium.Testing
{
  public interface IMvcAutoMocker : IAutoMocker
  {
    HttpContextBase CreateHttpContext();
    ActionDescriptor CreateActionDescriptor();
  }
}