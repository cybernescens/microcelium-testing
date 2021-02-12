using NHibernate;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  ///   Decorates a test so it is aware it will need to interact with the <see cref="ISession" />
  /// </summary>
  [RequireSessionFactory]
  public interface IRequireSessionFactory
  {
    /// <summary>
    ///   The instance that provides access to the SessionFactory 
    /// </summary>
    ISessionFactoryManager SessionFactoryManager { get; }
  }
}