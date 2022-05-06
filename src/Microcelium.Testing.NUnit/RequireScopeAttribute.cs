using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing;

/// <summary>
/// Applied to a class when the <see cref="IServiceProvider"/> should create
/// an <see cref="IServiceScope"/> around each test rather even if the
/// <see cref="LifeCycle"/> defined for the fixture is <see cref="LifeCycle.SingleInstance"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class RequireScopeAttribute : TestActionAttribute
{
  private IServiceScope serviceScope = null!;

  public override void BeforeTest(ITest test)
  {
    if (!test.Fixture!.GetType().IsAssignableTo(typeof(IRequireHost)))
      throw new Exception(
        $"Test should implement interface '{typeof(IRequireHost).FullName}'" +
        $" while also using the attribute '{typeof(RequireScopeAttribute).FullName}'");

    var attr = test.Fixture!.GetType().GetCustomAttributes().OfType<RequireHostAttribute>().FirstOrDefault();
    if (attr == null) 
      throw new Exception(
        $"Test should be decorated with attribute that inherits '{typeof(RequireHostAttribute).FullName}'" +
        $" prior to '{typeof(RequireScopeAttribute).FullName}'");

    serviceScope = attr.Host.Services.CreateScope();
    
    if (test.Fixture is IRequireServices sp)
      sp.Provider = serviceScope.ServiceProvider;
  }

  public override void AfterTest(ITest test)
  {
    SafelyTry.Dispose(() => serviceScope);
  }

  public override ActionTargets Targets => ActionTargets.Test;
}