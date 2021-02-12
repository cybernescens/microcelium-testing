using FluentAssertions;

namespace Microcelium.Testing.Acceptance
{
  internal class NUnitScenarioAttributeRunsATest
  {
    [Scenario]
    public void ShouldRun() => true.Should().BeTrue();
  }
}