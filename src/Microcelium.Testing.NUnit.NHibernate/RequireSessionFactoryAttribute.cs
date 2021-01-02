using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  ///   Used to decorate a class to provider NHibernate support / access.
  ///   When attached to a class then fires at the beginning and end of
  ///   running the suite. When attached to a method, fires at the start
  ///   and end of that method
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class RequireSessionFactoryAttribute : Attribute, ITestAction, IRequireLogger
  {
    private ILogger log;

    public void BeforeTest(ITest test)
    {
      var fixture = GetRequireSessionFactoryFromFixture(test);
      log = this.CreateLogger();
      var start = DateTime.Now;
      log.LogInformation("Initializing Session Factory...");
      fixture.SessionFactoryManager.Initialize();
      log.LogInformation($"SessionFactory initialized, took {(DateTime.Now - start).TotalSeconds:n2}s");

      fixture.OpenSession = fixture.SessionFactoryManager.SessionProvider;

      if (fixture is ISetupData data)
      {
        log.LogInformation("Invoking SetupData");
        data.SetupData();
      }
    }

    public void AfterTest(ITest test)
    {
      var fixture = GetRequireSessionFactoryFromFixture(test);
      fixture.OpenSession = fixture.SessionFactoryManager.SessionProvider;

      if (fixture is ISetupData data)
      {
        log.LogInformation("Invoking SetupData");
        data.CleanupData();
      }

      SafelyTry.Dispose(fixture.SessionFactoryManager);
    }

    public ActionTargets Targets => ActionTargets.Suite;

    private IRequireSessionFactory GetRequireSessionFactoryFromFixture(ITest test)
      => !(test.Fixture is IRequireSessionFactory requireSessionFactory)
        ? throw new Exception(
          $"Test should implement interface '{typeof(IRequireSessionFactory).FullName}'"
          + $" instead of using the attribute '{GetType().FullName}'")
        : requireSessionFactory;
  }
}