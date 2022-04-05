using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microcelium.Testing.Data.EntityFramework;

///// <summary>
///// Marks a fixture as requiring both a <see cref="DbContext"/> and a generic host;
///// uses the generic version of <see cref="DbContext"/>
///// </summary>
//public interface IRequireDbContext : IRequireHost
//{
//  /// <summary>
//  /// The Entity Framework data context
//  /// </summary>
//  DbContext DbContext { get; set; }
//}

/// <summary>
/// Marks a fixture as requiring both a <see cref="DbContext"/> and a generic host;
/// Uses a superclass version of <see cref="DbContext" />
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IRequireDbContext<TContext> : IRequireHost /*: IRequireDbContext*/ where TContext : DbContext
{
  /// <summary>
  /// The Entity Framework superclass data context
  /// </summary>
  IDbContextFactory<TContext> DbContextFactory { get; set; }
}

/// <summary>
/// Marks a fixture as requiring an instance of an in memory database
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IRequireInMemoryDbContext<TContext> : IRequireDbContext<TContext> where TContext : DbContext
{
  /// <summary>
  /// the shared root
  /// </summary>
  InMemoryDatabaseRoot DatabaseRoot { get; set; }

  /// <summary>
  /// The name of the database
  /// </summary>
  string DatabaseName { get; set; }
}

/// <summary>
/// Marks a fixture as requiring an instance of Sqlite
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IRequireSqliteDbContext<TContext> : IRequireDbContext<TContext> where TContext : DbContext
{
  /// <summary>
  /// Connection string so that the test initialization and test running can share a database
  /// </summary>
  string ConnectionString { get; }
}