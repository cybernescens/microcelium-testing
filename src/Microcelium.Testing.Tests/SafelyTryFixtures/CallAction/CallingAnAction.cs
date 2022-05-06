using FluentAssertions;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallAction;

[RequireGenericHost]
internal class CallingAnAction : IRequireLogValidation, IRequireLogging
{
  private bool wasCalled;

  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<CallingAnAction>();
    wasCalled = false;
    SafelyTry.Action(() => WasCalled(), log);
  }

  private void WasCalled() => wasCalled = true;

  [Test]
  public void CallsTheAction() => wasCalled.Should().BeTrue();

  [Test]
  public void WritesPreActionToTraceListener() =>
    LogContext.Received("Attempting action '.+?'", LogLevel.Debug, MatchMode.Regex);

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}