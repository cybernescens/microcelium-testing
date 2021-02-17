using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extension Methods for getting <see cref="IServiceCollection"/> or <see cref="IServiceProvider"/>
  /// </summary>
  public static class RequireServicesExtensions
  {
    /// <summary>
    /// Gets the current context's <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IServiceCollection GetServices(this IRequireServicesCollection _)
    {
      var services = (IServiceCollection)TestExecutionContext
        .CurrentContext
        .GetSuiteProperty(ManageServiceCollectionExtensions.ServiceCollectionPropertyKey);

      return services;
    }

    /// <summary>
    /// Gets the current context's <see cref="IServiceProvider"/>
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IServiceProvider GetProvider(this IRequireServicesCollection _)
    {
      var sp = (IServiceProvider)TestExecutionContext
        .CurrentContext
        .GetSuiteProperty(ManageServiceCollectionExtensions.ServiceProviderPropertyKey);

      return sp;
    }

    /// <summary>
    /// Attempts to get a required service of <typeparamref name="T"/> from the current context's <see cref="IServiceProvider"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <returns></returns>
    public static T GetRequired<T>(this IRequireServicesCollection _)
    {
      var sp = GetProvider(_);
      if (sp == null)
        throw new InvalidOperationException(
          "No ServiceProvider exists. Have not called IConfigureServicesCollection.BuildServiceProvider");

      return sp.GetRequiredService<T>();
    }

    /// <summary>
    /// Attempts to get a service of <typeparamref name="T"/> from the current context's <see cref="IServiceProvider"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <returns></returns>
    public static T Get<T>(this IRequireServicesCollection _)
    {
      var sp = GetProvider(_);
      if (sp == null)
        throw new InvalidOperationException(
          "No ServiceProvider exists. Have not called IConfigureServicesCollection.BuildServiceProvider");

      return sp.GetRequiredService<T>();
    }
  }
}