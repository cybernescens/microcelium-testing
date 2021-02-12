﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  ///   Used to decorate a class to provide NHibernate support / access.
  ///   When attached to a class then fires at the beginning and end of
  ///   running the suite. When attached to a method, fires at the start
  ///   and end of that method
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class RequireSessionFactoryAttribute :
    Attribute,
    ITestAction,
    IRequireLogger,
    IManageServiceCollection,
    IRequireServicesCollection
  {
    private ILogger log;
    private IServiceProvider provider;
    private IServiceScope scope;

    /// <inheritdoc />
    public void BeforeTest(ITest test)
    {
      var fixture = GetRequireSessionFactoryFromFixture(test);
      var services = this.GetServiceCollection();
      log = this.CreateLogger();

      var configProvider = test.Fixture as IProvideServiceCollectionConfiguration;
      configProvider?.Configure(services);

      services.TryAddSingleton(fixture.SessionFactoryManager);
      services.TryAddSingleton(
        sp => {
          var start = DateTime.Now;
          log.LogInformation("Initializing Session Factory...");
          var sf = sp.GetRequiredService<ISessionFactoryManager>().Initialize();
          log.LogInformation($"SessionFactory initialized, took {(DateTime.Now - start).TotalSeconds:n2}s");
          return sf;
        });

      services.TryAddScoped(sp => sp.GetRequiredService<ISessionFactoryManager>().SessionProvider);
      services.TryAddScoped(sp => sp.GetRequiredService<Func<ISession>>()());

      provider = this.BuildServiceProvider();

      if (test.Fixture is ISetupData data)
      {
        log.LogInformation("Invoking SetupData");
        using var pre = provider.CreateScope();
        using var session = provider.GetRequiredService<ISession>();
        data.SetupData(session);
      }

      scope = provider.CreateScope();
    }

    /// <inheritdoc />
    public void AfterTest(ITest test)
    {
      scope?.Dispose();

      if (test.Fixture is ISetupData data)
      {
        log.LogInformation("Invoking SetupData");
        using var post = provider.CreateScope();
        using var session = provider.GetRequiredService<ISession>();
        data.CleanupData(session);
      }

      provider.GetRequiredService<ISessionFactory>()?.Dispose();
      provider.GetRequiredService<ISessionFactoryManager>()?.Dispose();
    }

    /// <inheritdoc />
    public ActionTargets Targets => ActionTargets.Suite;

    private IRequireSessionFactory GetRequireSessionFactoryFromFixture(ITest test) =>
      test.Fixture is not IRequireSessionFactory requireSessionFactory
        ? throw new Exception(
          $"Test should implement interface '{typeof(IRequireSessionFactory).FullName}'" +
          $" instead of using the attribute '{GetType().FullName}'")
        : requireSessionFactory;
  }
}