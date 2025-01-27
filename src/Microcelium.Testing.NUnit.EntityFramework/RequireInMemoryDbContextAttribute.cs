﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Data.EntityFramework;

public class RequireInMemoryDbContextAttribute<TContext> : RequireDbContextAttribute<TContext> where TContext : DbContext
{
  private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  private static readonly string DatabaseNamePrefix = "testing-";
  private static readonly int DatabaseNameLength = 12;

  protected override void EnsureFixture(ITest test)
  {
    base.EnsureFixture(test);
    EnsureFixture<RequireInMemoryDbContextAttribute<TContext>, IRequireInMemoryDbContext<TContext>>(test);
  }

  protected override void AddEntityFramework(IServiceCollection services)
  {
    services.AddEntityFrameworkInMemoryDatabase();
  }

  protected override void ApplyContextProvider(ITest test, IHostBuilder builder, DbContextOptionsBuilder options)
  {
    var name = GetDatabaseName(); 
    var root = new InMemoryDatabaseRoot();

    ((IRequireInMemoryDbContext<TContext>)test.Fixture!).DatabaseRoot = root;
    ((IRequireInMemoryDbContext<TContext>)test.Fixture!).DatabaseName = name;

    options.UseInMemoryDatabase(
      name,
      root,
      configure => {
        if (test.Fixture is IConfigureInMemoryDbContext cfg)
          cfg.Configure(configure);
      });

    options.UseInternalServiceProvider(Host.Services);
  }

  private static string GetDatabaseName() =>
    DatabaseNamePrefix +
    new string(
      Enumerable.Range(0, DatabaseNameLength)
        .Select(_ => Characters[new Random().Next(0, Characters.Length)])
        .ToArray());
}