using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs;

[RequireGenericHost]
[Parallelizable(ParallelScope.Children)]
[TestFixture(typeof(WindsorAutoMockingContainer))]
internal class AutoMockingTestFixture<TAutoMocker> : 
  AutoMockSpecFor<FakeTestSubject, (int i, string s), TAutoMocker> 
  where TAutoMocker : IAutoMocker, new()
{
  protected override FakeTestSubject Arrange(Func<FakeTestSubject> createSubject)
  {
    ResolveDependency<IFakeDependency>().CreateInteger().Returns(5);
    RegisterDependency<IImplementedDependency, ImplementedDependency>();
    return base.Arrange(createSubject);
  }

  protected override (int i, string s) Act(FakeTestSubject subject) => subject.GetResult();

  [Test]
  public void ResultFromDependencyIsAsExpected() => Result.i.Should().Be(5);

  [Test]
  public void ResultFromImplementedDependencyIsAsExpected() => Result.s.Should().Be("Hello world");
}

public class FakeTestSubject
{
  private readonly IFakeDependency fakeDependency;
  private readonly IImplementedDependency implementedDependency;

  public FakeTestSubject(IFakeDependency fakeDependency, IImplementedDependency implementedDependency)
  {
    this.fakeDependency = fakeDependency;
    this.implementedDependency = implementedDependency;
  }

  public (int, string) GetResult() => (fakeDependency.CreateInteger(), implementedDependency.CreateString());
}

public class ImplementedDependency : IImplementedDependency
{
  //public ImplementedDependency() { }

  public string CreateString() => "Hello world";
}

public interface IImplementedDependency
{
  string CreateString();
}

public interface IFakeDependency
{
  int CreateInteger();
}