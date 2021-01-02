using System;
using NHibernate;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  ///   Provides the mechanism to open sessions from the <see cref="ISessionFactory" />
  /// </summary>
  public interface ISessionFactoryManager : IDisposable
  {
    /// <summary>
    /// Initializes the <see cref="ISessionFactory"/>
    /// </summary>
    ISessionFactory Initialize();

    /// <summary>
    ///   Provides the <see cref="ISessionFactory" />'s mechanism to open an <see cref="ISession" />
    /// </summary>
    Func<ISession> SessionProvider { get; }
  }
}