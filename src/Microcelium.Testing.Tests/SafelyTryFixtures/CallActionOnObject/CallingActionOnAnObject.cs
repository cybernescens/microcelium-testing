using FluentAssertions;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallActionOnObject;

[Parallelizable(ParallelScope.None)]
[RequireGenericHost]
internal class CallingActionOnAnObject : IRequireLogValidation, IRequireLogging
{
  private TestObject testObject;

  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<CallingActionOnAnObject>();
    testObject = new TestObject();
    SafelyTry.Action(testObject, x => x.Call(), log);
  }

  [Test]
  public void CallsTheAction() => testObject.WasCalled.Should().BeTrue();

  [Test]
  public void WritesPreActionToTraceListener() =>
    LogContext.Received("Attempting action '.+'", LogLevel.Debug, MatchMode.Regex);

  private class TestObject
  {
    public bool WasCalled { get; private set; }
    public void Call() => WasCalled = true;
  }

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}