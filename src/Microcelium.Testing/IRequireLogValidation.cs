using Microcelium.Testing.Logging;

namespace Microcelium.Testing;

/// <summary>
///   Initialize your fixture with this interface to initialize the infrastructure
///   required to capture log information
///   <para>
///     <strong>When using NUnit you'll need the require host attribute</strong>
///   </para>
/// </summary>
public interface IRequireLogValidation : IRequireHost
{
  /// <summary>
  ///   Context that allows us to make assertions
  /// </summary>
  LogValidationContext LogContext { get; set; }
}