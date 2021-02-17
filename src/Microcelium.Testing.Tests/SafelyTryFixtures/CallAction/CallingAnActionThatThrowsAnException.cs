using System;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallAction
{
  [Parallelizable(ParallelScope.None)]
  internal class CallingAnActionThatThrowsAnException : IRequireLogValidation, IManageLogging, IRequireLogger
  {
    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      this.AddLogging();
      var log = this.CreateLogger();
      SafelyTry.Action(() => ThrowsException(), log);
    }

    private void ThrowsException() => throw new Exception();

    [Test]
    public void WritesPreActionToTraceListener() =>
      LogContext.Received("Attempting action '.+?'", LogLevel.Debug, MatchMode.Regex);

    [Test]
    public void WritesErrorToTraceListener() =>
      LogContext.Received("Failed to perform action '.+'", LogLevel.Error, MatchMode.Regex, new Exception());
  }
}