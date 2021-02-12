using System;
using Microsoft.EntityFrameworkCore;

namespace Microcelium.Testing.NUnit.EntityFramework
{
  /// <summary>
  /// Manages the configuration and initialization of any <see cref="DbContext"/>s
  /// </summary>
  public interface IDbContextManager : IDisposable
  {
    /// <summary>
    /// Initializes the <see cref="DbContext"/>
    /// </summary>
    IDbContextFactory<DbContext> Initialize();

    /// <summary>
    ///   Provides the <see cref="IDbContextFactory{TContext}" />'s mechanism to open an <see cref="DbContext" />
    /// </summary>
    Func<DbContext> ContextProvider { get; }
  }
}