using System;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallDispose;

[Parallelizable(ParallelScope.None)]
[RequireGenericHost]
internal class DisposingAnObjectThatThrowsAnException : IRequireLogValidation, IRequireLogging
{
  private IDisposable disposable;

  public LogValidationContext LogContext { get; set; }

  [SetUp]
  public void SetUp()
  {
    var log = LoggerFactory.CreateLogger<DisposingAnObjectThatThrowsAnException>();
    disposable = Substitute.For<IDisposable>();
    disposable.When(x => x.Dispose()).Throw<Exception>();
    SafelyTry.Dispose(disposable, log);
  }

  [Test]
  public void CallsTheDisposeMethod() => disposable.Received().Dispose();

  [Test]
  public void WritesPreActionToTraceListener() =>
    LogContext.Received("Attempting action '.+'", LogLevel.Debug, MatchMode.Regex);

  [Test]
  public void WritesErrorToTraceListener() =>
    LogContext.Received("Failed to perform action '.+'", LogLevel.Error, MatchMode.Regex, new Exception());

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
}