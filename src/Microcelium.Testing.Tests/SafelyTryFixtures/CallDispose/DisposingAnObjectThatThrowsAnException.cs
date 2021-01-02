using System;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallDispose
{
  [Parallelizable(ParallelScope.None)]
  internal class DisposingAnObjectThatThrowsAnException : IRequireLogValidation, IRequireLogger
  {
    private IDisposable disposable;

    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      var log = this.CreateLogger();
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
  }
}