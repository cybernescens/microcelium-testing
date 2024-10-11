using Microsoft.AspNetCore.Builder;

namespace Microcelium.Testing.Web;

public interface IConfigureWebHost : IRequireWebHost
{
  void Configure(WebApplicationBuilder builder);
}