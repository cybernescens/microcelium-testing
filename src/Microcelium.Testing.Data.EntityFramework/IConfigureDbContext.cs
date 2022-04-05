using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microcelium.Testing.Data.EntityFramework;

/// <summary>
/// Marks a fixture with ability to configure a <see cref="DbContext"/>
/// </summary>
public interface IConfigureDbContext
{
  /// <summary>
  /// Configures the DbContext via the <see cref="DbContextOptionsBuilder"/>
  /// </summary>
  /// <param name="builder">the <see cref="DbContextOptionsBuilder"/></param>
  void Configure(DbContextOptionsBuilder builder);
}

/// <summary>
/// Marks a fixture with the ability to configure an In-memory database context
/// </summary>
public interface IConfigureInMemoryDbContext
{
  /// <summary>
  /// Configures the <see cref="InMemoryDbContextOptionsBuilder"/>
  /// </summary>
  /// <param name="builder">the <see cref="InMemoryDbContextOptionsBuilder"/></param>
  void Configure(InMemoryDbContextOptionsBuilder builder);
}

/// <summary>
/// Marks a fixture with the ability to configure a Sqlite database context
/// </summary>
public interface IConfigureSqliteDbContext
{
  /// <summary>
  /// Configures the <see cref="SqliteDbContextOptionsBuilder"/>
  /// </summary>
  /// <param name="builder">the <see cref="SqliteDbContextOptionsBuilder"/></param>
  void Configure(SqliteDbContextOptionsBuilder builder);
}