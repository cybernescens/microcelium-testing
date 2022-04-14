using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Data.EntityFramework;

public class RequireSqliteDbContextAttribute<TContext> : RequireDbContextAttribute<TContext> where TContext : DbContext
{
  protected override void EnsureFixture(ITest test)
  {
    base.EnsureFixture(test);
    EnsureFixture<RequireSqliteDbContextAttribute<TContext>, IRequireSqliteDbContext<TContext>>(test);
  }

  protected override void ApplyContextProvider(ITest test, IHostBuilder builder, DbContextOptionsBuilder options)
  {
    var connectionString = ((IRequireSqliteDbContext<TContext>)test.Fixture!).ConnectionString;

    if (string.IsNullOrEmpty(connectionString))
      Assert.Fail(
        $"{nameof(IRequireSqliteDbContext<TContext>)}.{nameof(IRequireSqliteDbContext<TContext>.ConnectionString)} must be specified.");

    options.UseSqlite(
      connectionString,
      configure => {
        if (test.Fixture is IConfigureSqliteDbContext cfg)
          cfg.Configure(configure);
      });
  }
}