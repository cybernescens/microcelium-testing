using FluentAssertions;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallAction
{
  [Parallelizable(ParallelScope.None)]
  internal class CallingAnAction : IRequireLogValidation, IRequireLogger, IManageLogging
  {
    private bool wasCalled;

    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      this.AddLogging();
      var log = this.CreateLogger();
      wasCalled = false;
      SafelyTry.Action(() => WasCalled(), log);
    }

    private void WasCalled() => wasCalled = true;

    [Test]
    public void CallsTheAction() => wasCalled.Should().BeTrue();

    [Test]
    public void WritesPreActionToTraceListener() => LogContext.Received("Attempting action '.+?'", LogLevel.Debug, MatchMode.Regex);
  }
}