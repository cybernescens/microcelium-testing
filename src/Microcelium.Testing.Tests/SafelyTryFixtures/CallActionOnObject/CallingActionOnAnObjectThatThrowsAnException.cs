using System;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallActionOnObject
{
  [Parallelizable(ParallelScope.None)]
  internal class CallingActionOnAnObjectThatThrowsAnException : IRequireLogValidation, IRequireLogger
  {
    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      var log = this.CreateLogger();
      var testObject = new TestObject();
      SafelyTry.Action(testObject, x => x.CallWithException(), log);
    }

    [Test]
    public void WritesPreActionToTraceListener() =>
      LogContext.Received("Attempting action '.+'", LogLevel.Debug, MatchMode.Regex);

    [Test]
    public void WritesErrorToTraceListener() =>
      LogContext.Received("Failed to perform action '.+'", LogLevel.Error, MatchMode.Regex, new Exception());

    private class TestObject
    {
      public void CallWithException() => throw new Exception();
    }
  }
}