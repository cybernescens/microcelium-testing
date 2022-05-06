using System;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallActionOnObject;

[RequireGenericHost]
internal class CallingActionOnAnObjectThatThrowsAnException : IRequireLogValidation, IRequireLogging
{
  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<CallingActionOnAnObjectThatThrowsAnException>();
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
  
  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}