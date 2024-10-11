using System;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing;

public sealed class RequireGenericHostAttribute : RequireHostAttribute
{
  private IRequireHost fixture = null!;
  protected override IRequireHost Fixture => fixture;

  protected override IHostBuilder CreateHostBuilder() => new HostBuilder();
  
  protected override IHost CreateHost(IHostBuilder builder) => builder.Build();

  protected override void EnsureFixture(ITest test)
  {
    fixture = EnsureFixture<RequireGenericHostAttribute, IRequireHost>(test);
  }

  protected override void OnHostBuilt(ITest test)
  {
    Fixture.Host = host!;
  }

  protected override void ApplyToContext()
  {
    AddToContext(nameof(IRequireHost), Fixture.Host);
  }
}