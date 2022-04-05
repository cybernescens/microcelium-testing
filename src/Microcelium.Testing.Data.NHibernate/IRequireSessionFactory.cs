using NHibernate;

namespace Microcelium.Testing.Data.NHibernate;

/// <summary>
///   Decorates a test so it is aware it will need to interact with the <see cref="ISession" />
/// </summary>
public interface IRequireSessionFactory : IRequireHost
{
  /// <summary>
  ///   The instance that provides access to the SessionFactory
  /// </summary>
  ISessionFactory SessionFactory { get; set; }
}