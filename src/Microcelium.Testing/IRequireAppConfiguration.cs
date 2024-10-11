using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microcelium.Testing;

public interface IRequireAppConfiguration : IRequireHost { }

public interface IConfigureHostApplication
{
  void Apply(HostBuilderContext context, IConfigurationBuilder builder);
}