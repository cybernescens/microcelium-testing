using System;
using Microsoft.EntityFrameworkCore;

namespace Microcelium.Testing.NUnit.EntityFramework
{
  /// <summary>
  /// Decorator when it's necessary to create and manage an EntityFramework context for the tests
  /// </summary>
  [RequireDbContext]
  public interface IRequireDbContext
  {
    /// <summary>
    /// The <see cref="DbContext"/> manager
    /// </summary>
    IDbContextManager DbContextManager { get; }

    /// <summary>
    /// Function/Factory for getting an active  <see cref="DbContext"/>
    /// </summary>
    Func<DbContext> OpenContext { get; set; }
  }
}