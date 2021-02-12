using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit.EntityFramework
{
  /// <summary>
  ///   Used to decorate a class to provide EntityFramework support / access.
  ///   When attached to a class then fires at the beginning and end of
  ///   running the suite. When attached to a method, fires at the start
  ///   and end of that method
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class RequireDbContextAttribute : 
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
      var fixture = GetRequireDbContextFromFixture(test);
      var services = this.GetServiceCollection();
      log = this.CreateLogger();

      var configProvider = test.Fixture as IProvideServiceCollectionConfiguration;
      configProvider?.Configure(services);

      services.TryAddSingleton(fixture.DbContextManager);
      services.TryAddSingleton(
        sp => {
          var start = DateTime.Now;
          log.LogInformation("Initializing IDbContextManager...");
          var sf = sp.GetRequiredService<IDbContextManager>().Initialize();
          log.LogInformation($"IDbContextManager initialized, took {(DateTime.Now - start).TotalSeconds:n2}s");
          return sf;
        });

      services.TryAddScoped(sp => sp.GetRequiredService<IDbContextManager>().ContextProvider);
      services.TryAddScoped(sp => sp.GetRequiredService<Func<DbContext>>()());

      provider = this.BuildServiceProvider();

      if (test.Fixture is ISetupData data)
      {
        log.LogInformation("Invoking SetupData");
        using var pre = provider.CreateScope();
        using var session = provider.GetRequiredService<DbContext>();
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
        using var session = provider.GetRequiredService<DbContext>();
        data.CleanupData(session);
      }

      provider.GetRequiredService<IDbContextManager>()?.Dispose();
    }

    /// <inheritdoc />
    public ActionTargets Targets { get; }

    private IRequireDbContext GetRequireDbContextFromFixture(ITest test)
      => test.Fixture is not IRequireDbContext requireDbContext
        ? throw new Exception(
          $"Test should implement interface '{typeof(IRequireDbContext).FullName}'" + 
          $" instead of using the attribute '{GetType().FullName}'")
        : requireDbContext;
  }
}
