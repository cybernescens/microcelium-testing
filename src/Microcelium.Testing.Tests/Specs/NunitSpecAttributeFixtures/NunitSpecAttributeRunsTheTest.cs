using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Microcelium.Testing.Specs.NunitSpecAttributeFixtures;

[RequireGenericHost]
[Parallelizable(ParallelScope.Children)]
internal class NunitSpecAttributeRunsTheTest : SpecsFor<int, int>
{
  protected override int CreateSubject() => 15;

  protected override int Act(int subject) => subject;

  [Test]
  public void ArrangeAndActAreRun() => Result.Should().Be(15);
}

[RequireGenericHost]
[Parallelizable(ParallelScope.Children)]
internal class NunitSpecAttributeRunsTheTestAsync : AsyncSpecsFor<int, int>
{
  protected override Task<int> CreateSubject() => Task.FromResult(15);

  protected override Task<int> Act(int subject) => Task.FromResult(subject);

  [Test]
  public void ArrangeAndActAreRun() => Result.Should().Be(15);
}