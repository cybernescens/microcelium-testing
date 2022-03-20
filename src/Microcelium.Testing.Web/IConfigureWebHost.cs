using Microsoft.AspNetCore.Hosting;

namespace Microcelium.Testing.Web;

public interface IConfigureWebHost : IRequireWebHost
{
  void Configure(IWebHostBuilder builder);
}