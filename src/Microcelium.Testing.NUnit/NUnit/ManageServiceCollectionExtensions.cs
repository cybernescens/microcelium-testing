using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extensions for configuring a <see cref="IServiceCollection"/> and <see cref="IServiceProvider"/>
  /// </summary>
  public static class ManageServiceCollectionExtensions
  {
    /// <summary>
    /// Key for the <see cref="IServiceCollection"/>
    /// </summary>
    public static readonly string ServiceCollectionPropertyKey = nameof(IServiceCollection);

    /// <summary>
    /// Key forr the <see cref="IServiceProvider"/>
    /// </summary>
    public static readonly string ServiceProviderPropertyKey = nameof(IServiceProvider);

    /// <summary>
    /// Creates an <see cref="IServiceCollection"/>. Caution, will overwrite any existing one
    /// </summary>
    /// <param name="_">ignored</param>
    /// <param name="config">optional config</param>
    /// <returns></returns>
    public static IServiceCollection CreateServiceCollection(this IManageServiceCollection _, Action<IServiceCollection> config = null)
    {
      var services = new ServiceCollection();
      config?.Invoke(services);
      TestExecutionContext.CurrentContext.SetSuiteProperty(ServiceCollectionPropertyKey, services);
      return services;
    }

    /// <summary>
    /// Gets an <see cref="IServiceCollection"/> and will create one if none exists
    /// </summary>
    /// <param name="icsc"></param>
    /// <returns></returns>
    public static IServiceCollection GetServiceCollection(this IManageServiceCollection icsc)
    {
      var service = (IServiceCollection) TestExecutionContext
          .CurrentContext
          .GetSuiteProperty(ServiceCollectionPropertyKey) ??
        CreateServiceCollection(icsc);

      return service;
    }

    /// <summary>
    /// Builds a <see cref="IServiceProvider"/>. Caution, will overwrite any existing one
    /// </summary>
    /// <param name="icsc"></param>
    /// <returns></returns>
    public static IServiceProvider BuildServiceProvider(this IManageServiceCollection icsc)
    {
      var services = GetServiceCollection(icsc);
      var sp = services.BuildServiceProvider();
      TestExecutionContext.CurrentContext.SetSuiteProperty(ServiceProviderPropertyKey, sp);
      return sp;
    }

    /// <summary>
    /// Gets an <see cref="IServiceProvider"/> and will create one if none exists
    /// </summary>
    /// <param name="icsc"></param>
    /// <returns></returns>
    public static IServiceProvider GetServiceProvider(this IManageServiceCollection icsc)
    {
      var sp = (IServiceProvider) TestExecutionContext
          .CurrentContext
          .GetSuiteProperty(ServiceProviderPropertyKey) ??
        BuildServiceProvider(icsc);
      
      return sp;
    }
  }
}