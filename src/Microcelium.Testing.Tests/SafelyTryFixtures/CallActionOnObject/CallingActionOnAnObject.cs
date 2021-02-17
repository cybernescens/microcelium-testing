using FluentAssertions;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallActionOnObject
{
  [Parallelizable(ParallelScope.None)]
  internal class CallingActionOnAnObject : IRequireLogValidation, IManageLogging, IRequireLogger
  {
    private TestObject testObject;

    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      this.AddLogging();
      var log = this.CreateLogger();
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
  }
}