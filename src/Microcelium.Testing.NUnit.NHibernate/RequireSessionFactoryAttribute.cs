using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Data.NHibernate;

/// <summary>
///   Used to decorate a class to provider NHibernate support / access.
///   When attached to a class then fires at the beginning and end of
///   running the suite. When attached to a method, fires at the start
///   and end of that method
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RequireSessionFactoryAttribute : RequireHostAttribute
{
  private Configuration configuration = null!;
  private IRequireSessionFactory fixture = null!;
  private ISessionFactory sessionFactory = null!;

  protected override IRequireHost Fixture => fixture;

  protected override void EnsureFixture(ITest test)
  {
    fixture = EnsureFixture<RequireSessionFactoryAttribute, IRequireSessionFactory>(test);
  }

  protected override IHostBuilder CreateHostBuilder() => new HostBuilder();
  protected override IHost CreateHost(IHostBuilder builder) => builder.Build();

  protected override void OnBeforeCreateHost(IHostBuilder builder, ITest test)
  {
    configuration = new Configuration();
    if (test.Fixture is IConfigureSessionFactory sf)
      sf.Configure(configuration);
  }

  protected override void OnAfterCreateHost(ITest test)
  {
    sessionFactory = Host.Services.GetRequiredService<ISessionFactory>();
    ((IRequireSessionFactory)test.Fixture!).SessionFactory = sessionFactory;
  }

  protected override void OnEndBeforeTest(ITest test)
  {
    using var session = sessionFactory.OpenSession();
    if (test.Fixture is ISetupData data)
      data.SetupData(session);
  }

  protected override void OnStartAfterTest(ITest test)
  {
    using var session = sessionFactory.OpenSession();
    if (test.Fixture is ICleanupData data)
      data.CleanupData(session);
  }

  protected override void DefaultServicesConfiguration(HostBuilderContext ctx, IServiceCollection services)
  {
    services.AddSingleton(configuration);
    services.AddSingleton(sp => sp.GetRequiredService<Configuration>().BuildSessionFactory());
  }
}