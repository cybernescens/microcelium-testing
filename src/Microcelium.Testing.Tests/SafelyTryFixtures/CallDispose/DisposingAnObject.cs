using System;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallDispose;

[Parallelizable(ParallelScope.None)]
[RequireGenericHost]
internal class DisposingAnObject : IRequireLogValidation, IRequireLogging
{
  private IDisposable disposable;

  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<DisposingAnObject>();
    disposable = Substitute.For<IDisposable>();
    SafelyTry.Dispose(disposable, log);
  }

  [Test]
  public void CallsTheDisposeMethod() => disposable.Received().Dispose();

  [Test]
  public void WritesPreActionToTraceListener() =>
    LogContext.Received("Attempting action '.+'", LogLevel.Debug, MatchMode.Regex);

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}