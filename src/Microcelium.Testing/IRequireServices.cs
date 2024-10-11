using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microcelium.Testing;

public interface IRequireServices : IRequireHost
{
  IServiceProvider Provider { get; set; }
}

public interface IConfigureServices
{
  void Apply(HostBuilderContext context, IServiceCollection services);
}
