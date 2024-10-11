using Microsoft.Extensions.Hosting;

namespace Microcelium.Testing;

public interface IRequireHost
{
  IHost Host { get; set; }
}

public interface IConfigureHost : IRequireHost
{
  void Configure(HostBuilderContext context, IHostBuilder builder);
}
