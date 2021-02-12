using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  ///   Extension methods for generating Loggers from tests
  /// </summary>
  public static class RequireLoggerExtensions
  {
    public static ILogger CreateLogger(this IRequireLogger fixture) => fixture.CreateLogger(fixture.GetType());

    /// <summary>
    ///   Creates a new <see cref="Microsoft.Extensions.Logging.ILogger" /> instance using the full name of the given type.
    /// </summary>
    /// <param name="fixture">The fixture.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The <see cref="Microsoft.Extensions.Logging.ILogger" /> that was created.</returns>
    public static ILogger CreateLogger<T>(this IRequireLogger fixture) => fixture.CreateLogger(typeof(T));

    /// <summary>
    ///   Creates a new <see cref="ILogger" /> instance using the full name of the given <paramref name="type" />.
    /// </summary>
    /// <param name="fixture">The fixture.</param>
    /// <param name="type">The type.</param>
    /// <return>The <see cref="ILogger" /> that was created.</return>
    public static ILogger CreateLogger(this IRequireLogger fixture, Type type)
    {
      var lf = fixture.GetLoggerFactory();
      return lf.CreateLogger(type);
    }

    /// <summary>
    ///   Creates an <see cref="ILogger" /> with the given <paramref name="categoryName" />.
    /// </summary>
    /// <param name="fixture">The fixture.</param>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>The <see cref="ILogger" /> that was created.</returns>
    public static ILogger CreateLogger(this IRequireLogger fixture, string categoryName)
    {
      var lf = fixture.GetLoggerFactory();
      return lf.CreateLogger(categoryName);
    }

    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/>
    /// </summary>
    /// <param name="_">ignored</param>
    /// <returns></returns>
    public static ILoggerFactory GetLoggerFactory(this IRequireLogger _)
    {
      var lf = (ILoggerFactory)TestExecutionContext
        .CurrentContext
        .GetSuiteProperty(ManageLoggingExtensions.LoggerFactoryPropertyKey);

      return lf;
    }
  }

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