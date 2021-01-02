using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using Serilog;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extension Helpers to Add logging and properly add the <see cref="ILoggerFactory"/>
  ///   to the NUnit <see cref="TestExecutionContext"/>
  /// </summary>
  public static class ConfigureLoggingExtensions
  {
    /// <summary>
    /// Key used for the <see cref="IPropertyBag"/>
    /// </summary>
    public static readonly string PropertyKey = nameof(ILoggerFactory);

    /// <summary>
    /// Adds logging and ensures there exists a contextual <see cref="ILoggerFactory"/> we can
    ///   have access to within tests
    /// </summary>
    /// <param name="_">the <see cref="IConfigureLogging"/> decorator</param>
    /// <param name="services">a <see cref="IServiceCollection"/></param>
    public static IServiceCollection AddLogging(this IConfigureLogging _, IServiceCollection services = null)
    {
      services ??= new ServiceCollection();
      services.AddLogging(lb => lb.AddSerilog(dispose: true));
      var sp = services.BuildServiceProvider();
      var factory = sp.GetService<ILoggerFactory>();

      TestExecutionContext.CurrentContext.CurrentTest.Properties.Set(PropertyKey, factory);
      return services;
    }
  }
}