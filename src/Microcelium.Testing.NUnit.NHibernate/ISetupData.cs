using NHibernate;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  /// Implement this interface for data to automatically be picked up
  /// </summary>
  public interface ISetupData
  {
    /// <summary>
    ///   Invoked before the test is ran to set up any test data
    /// </summary>
    /// <param name="session"></param>
    void SetupData(ISession session);

    /// <summary>
    ///   Invoked after the test is to do any optional cleanup (should be destroyed with test)
    /// </summary>
    /// <param name="session"></param>
    void CleanupData(ISession session);
  }
}