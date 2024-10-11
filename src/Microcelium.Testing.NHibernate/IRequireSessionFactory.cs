using NHibernate;
using NHibernate.Cfg;

namespace Microcelium.Testing.NHibernate;

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

/// <summary>
///   Provides access to the <see cref="NHibernate.Cfg.Configuration" />
/// </summary>
public interface IConfigureSessionFactory
{
  /// <summary>
  ///   Exposes an <see cref="NHibernate.Cfg.Configuration" /> object
  ///   that an <see cref="ISessionFactory" /> is built from.
  /// </summary>
  /// <param name="configuration">the <see cref="NHibernate.Cfg.Configuration" /> instance</param>
  void Configure(Configuration configuration);
}