using Microsoft.Extensions.DependencyInjection;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Enableds configuration of the test specific container instance
  /// </summary>
  public interface IProvideServiceCollectionConfiguration : IRequireServicesCollection
  {
    /// <summary>
    /// Configures the test specific container instance
    /// </summary>
    /// <param name="services">the <see cref="IServiceCollection"/></param>
    void Configure(IServiceCollection services);
  }
}