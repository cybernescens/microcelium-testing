using NHibernate;

namespace Microcelium.Testing.Data.NHibernate;

/// <summary>
///   Implement this interface for data to automatically be picked up
/// </summary>
public interface ISetupData
{
  /// <summary>
  ///   Invoked before the test is ran to set up any test data
  /// </summary>
  void SetupData(ISession session);
}