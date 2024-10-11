using Microsoft.AspNetCore.Builder;

namespace Microcelium.Testing.Web;

public interface IRequireWebHostOverride : IRequireWebHost
{
  void Configure(WebApplication endpoint);
}