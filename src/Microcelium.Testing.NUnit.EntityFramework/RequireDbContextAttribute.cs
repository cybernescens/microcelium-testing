using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Data.EntityFramework;

[AttributeUsage(AttributeTargets.Class)]
public abstract class RequireDbContextAttribute<TContext> : RequireHostAttribute where TContext : DbContext
{
  private IRequireDbContext<TContext> fixture = null!;
  private IDbContextFactory<TContext>? contextFactory;

  protected override IRequireHost Fixture => fixture;

  protected override void EnsureFixture(ITest test)
  {
    fixture = EnsureFixture<RequireDbContextAttribute<TContext>, IRequireDbContext<TContext>>(test);
  }

  protected override IHostBuilder CreateHostBuilder() => new HostBuilder();
  protected override IHost CreateHost(IHostBuilder builder) => builder.Build();

  protected override void OnBeforeCreateHost(IHostBuilder builder, ITest test)
  {
    builder.ConfigureServices(
      services => {
        AddEntityFramework(services);
        services.AddDbContextFactory<TContext>(
          options => {
            ApplyContextProvider(test, builder, options);

            if (test.Fixture is IConfigureDbContext cfg)
              cfg.Configure(options);
          });
      });
  }

  protected abstract void AddEntityFramework(IServiceCollection services);

  protected override void OnAfterCreateHost(ITest test)
  {
    contextFactory = Host.Services.GetRequiredService<IDbContextFactory<TContext>>();
    ((IRequireDbContext<TContext>)test.Fixture!).DbContextFactory = contextFactory;
  }

  protected override void OnEndBeforeTest(ITest test)
  {
    using var context = contextFactory!.CreateDbContext();

    if (test.Fixture is IEnsureSchema)
      context.Database.EnsureCreated();

    if (test.Fixture is ISetupData<TContext> data)
      data.SetupData(context);
  }

  protected override void OnStartAfterTest(ITest test)
  {
    if (contextFactory == null)
      return;

    using var context = contextFactory.CreateDbContext();
    
    if (test.Fixture is ICleanupData<TContext> data)
      data.CleanupData(context);

    if (test.Fixture is IEnsureSchema)
      context.Database.EnsureDeleted();
  }

  protected abstract void ApplyContextProvider(ITest test, IHostBuilder builder, DbContextOptionsBuilder options);
}