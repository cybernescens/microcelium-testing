using Microsoft.EntityFrameworkCore;

namespace Microcelium.Testing.Data.EntityFramework;

/// <summary>
/// Decorate your fixture with this class to set up data prior to the test
/// </summary>
public interface ICleanupData<in TContext> where TContext : DbContext
{
  /// <summary>
  /// Cleans up data
  /// </summary>
  void CleanupData(TContext context);
}