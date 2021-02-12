using Microsoft.EntityFrameworkCore;

namespace Microcelium.Testing.NUnit.EntityFramework
{
  /// <summary>
  /// Implement this interface for data to automatically be picked up
  /// </summary>
  public interface ISetupData
  {
    /// <summary>``
    ///   Invoked before the test is ran to set up any test data
    /// </summary>
    /// <param name="dbContext"></param>
    void SetupData(DbContext dbContext);

    /// <summary>
    ///   Invoked after the test is to do any optional cleanup (should be destroyed with test)
    /// </summary>
    /// <param name="dbContext"></param>
    void CleanupData(DbContext dbContext);
  }
}