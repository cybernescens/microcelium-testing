using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microcelium.Testing.NUnit.AspNetCore.TestServer
{
  [RequiresTestEndpoint]
  public interface IRequireTestEndpoint
  {
    Microsoft.AspNetCore.TestHost.TestServer Endpoint { get; set; }
  }

  public interface IRequireTestEndpointStartup<TStartup> : IRequireTestEndpoint where TStartup : class { }

  public interface IRequireTestEndpointServices : IRequireTestEndpoint
  {
    void Configure(IServiceCollection services);
  }

  public interface IRequireTestEndpointApplicationBuilder : IRequireTestEndpoint
  {
    void Configure(IApplicationBuilder builder);
  }

  public interface IRequireTestEndpointHostBuilder : IRequireTestEndpoint
  {
    void Configure(IWebHostBuilder builder);
  }

  public interface IRequireTestEndpointOverride : IRequireTestEndpoint
  {
    Task ServerRun(HttpContext context);
  }
}