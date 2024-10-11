namespace Microcelium.Testing.NHibernate;

/// <summary>
/// Decorate your fixture with this class to set up data prior to the test
/// </summary>
public interface ICleanupData
{
  /// <summary>
  /// 
  /// </summary>
  void CleanupData();
}