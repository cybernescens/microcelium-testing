using Microcelium.Testing.Logging;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit
{
  [Parallelizable(ParallelScope.None)]
  internal class LogTestNameAttributeFixtures : IRequireLogValidation, IManageLogging
  {
    private ITest fakeTest;
    private TraceTestNameAttribute traceTestNameAttribute;

    public LogTestContext LogContext { get; set; }

    [SetUp]
    public void SetUp()
    {
      this.AddLogging();
      traceTestNameAttribute = new TraceTestNameAttribute();
      fakeTest = Substitute.For<ITest>();
      fakeTest.FullName.Returns("Foo");
    }

    [Test]
    public void TracesTestNameBeforeTestRuns()
    {
      traceTestNameAttribute.BeforeTest(fakeTest);
      LogContext.Received("Before: Foo", mode: MatchMode.Exact);
    }

    [Test]
    public void TracesTestNameAfterTestRuns()
    {
      traceTestNameAttribute.AfterTest(fakeTest);
      LogContext.Received("After: Foo", mode: MatchMode.Exact);
    }
  }
}