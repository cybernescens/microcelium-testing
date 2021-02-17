using System;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.SafelyTryFixtures.CallDispose
{
  [Parallelizable(ParallelScope.None)]
  internal class DisposingAnObject : IRequireLogValidation, IManageLogging, IRequireLogger
  {
    private IDisposable disposable;

    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      this.AddLogging();
      var log = this.CreateLogger();
      disposable = Substitute.For<IDisposable>();
      SafelyTry.Dispose(disposable, log);
    }

    [Test]
    public void CallsTheDisposeMethod() => disposable.Received().Dispose();

    [Test]
    public void WritesPreActionToTraceListener() =>
      LogContext.Received("Attempting action '.+'", LogLevel.Debug, MatchMode.Regex);
  }
}