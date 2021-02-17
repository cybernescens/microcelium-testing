using System;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing
{
  /// <summary>
  /// Decorative interface to ensure access to the <see cref="ILoggerFactory" />.
  /// Access is made available via an extension method in <see cref="RequireLoggerExtensions"/>
  /// </summary>
  public interface IRequireLogger { }
}