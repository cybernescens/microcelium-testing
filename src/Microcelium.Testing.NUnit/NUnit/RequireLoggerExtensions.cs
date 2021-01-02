using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extension methods for generating Loggers from tests
  /// </summary>
  public static class RequireLoggerExtensions
  {
    public static ILogger CreateLogger(this IRequireLogger fixture) => fixture.CreateLogger(fixture.GetType());

    /// <summary>
    /// Creates a new <see cref="Microsoft.Extensions.Logging.ILogger"/> instance using the full name of the given type.
    /// </summary>
    /// <param name="fixture">The fixture.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The <see cref="Microsoft.Extensions.Logging.ILogger"/> that was created.</returns>
    public static ILogger CreateLogger<T>(this IRequireLogger fixture) => fixture.CreateLogger(typeof(T));

    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance using the full name of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="_">The fixture.</param>
    /// <param name="type">The type.</param>
    /// <return>The <see cref="ILogger"/> that was created.</return>
    public static ILogger CreateLogger(this IRequireLogger _, Type type)
    {
      var lf = (ILoggerFactory) TestExecutionContext
        .CurrentContext
        .GetSuiteProperty<IConfigureLogging>(ConfigureLoggingExtensions.PropertyKey);

      return lf.CreateLogger(type);
    }

    /// <summary>
    /// Creates an <see cref="ILogger"/> with the given <paramref name="categoryName"/>.
    /// </summary>
    /// <param name="_">The fixture.</param>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>The <see cref="ILogger"/> that was created.</returns>
    public static ILogger CreateLogger(this IRequireLogger _, string categoryName)
    {
      var lf = (ILoggerFactory) TestExecutionContext
        .CurrentContext
        .GetSuiteProperty<IConfigureLogging>(ConfigureLoggingExtensions.PropertyKey);

      return lf.CreateLogger(categoryName);
    }
  }
}

  