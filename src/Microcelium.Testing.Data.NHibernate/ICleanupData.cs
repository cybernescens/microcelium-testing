using NHibernate;

namespace Microcelium.Testing.Data.NHibernate;

/// <summary>
/// Decorate your fixture with this class to set up data prior to the test
/// </summary>
public interface ICleanupData
{
  /// <summary>
  /// 
  /// </summary>
  void CleanupData(ISession session);
}