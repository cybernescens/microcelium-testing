using NHibernate;
using NHibernate.Cfg;

namespace Microcelium.Testing.Data.NHibernate;

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