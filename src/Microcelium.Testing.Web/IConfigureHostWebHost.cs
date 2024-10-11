using Microsoft.AspNetCore.Hosting;

namespace Microcelium.Testing.Web;

public interface IConfigureHostWebHost : IRequireWebHost
{
  void Configure(IWebHostBuilder builder);
}