using Microsoft.EntityFrameworkCore;

namespace Microcelium.Testing.Data.EntityFramework;

/// <summary>
///   Implement this interface for data to automatically be picked up
/// </summary>
public interface ISetupData<in TContext> where TContext : DbContext
{
  /// <summary>
  ///   Invoked before the test is ran to set up any test data
  /// </summary>
  void SetupData(TContext context);
}