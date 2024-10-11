using System;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallAction;

[Parallelizable(ParallelScope.None)]
[RequireGenericHost]
internal class CallingAnActionThatThrowsAnException : IRequireLogValidation, IRequireLogging
{
  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<CallingAnActionThatThrowsAnException>();
    SafelyTry.Action(() => ThrowsException(), log);
  }

  private void ThrowsException() => throw new Exception();

  [Test]
  public void WritesPreActionToTraceListener() =>
    LogContext.Received("Attempting action '.+?'", LogLevel.Debug, MatchMode.Regex);

  [Test]
  public void WritesErrorToTraceListener() =>
    LogContext.Received("Failed to perform action '.+'", LogLevel.Error, MatchMode.Regex, new Exception());

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}