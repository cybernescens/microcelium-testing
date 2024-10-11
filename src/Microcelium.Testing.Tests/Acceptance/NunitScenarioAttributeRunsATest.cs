using FluentAssertions;

namespace Microcelium.Testing.Acceptance;

internal class NunitScenarioAttributeRunsATest
{
  [Scenario]
  //This really isn't a great test. Id it didn't work the test wouldn't actually run and the test wouldn't fail
  //This is really here to exercise the attribute
  public void ShouldRun() => true.Should().BeTrue();
}