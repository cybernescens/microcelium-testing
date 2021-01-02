using System;
using System.Threading.Tasks;
using NHibernate;

namespace Microcelium.Testing.NUnit.NHibernate
{
  /// <summary>
  ///   Decorates a test so it is aware it will need to interact with the <see cref="ISession" />
  /// </summary>
  [RequireSessionFactory]
  public interface IRequireSessionFactory
  {
    /// <summary>
    ///   invoking this function property will oppen an <see cref="ISession" />
    /// </summary>
    Func<ISession> OpenSession { get; set; }

    /// <summary>
    ///   The instance that provides access to the SessionFactory 
    /// </summary>
    ISessionFactoryManager SessionFactoryManager { get; }
  }

  /// <summary>
  /// Implement this interface for data to automatically be picked up
  /// </summary>
  public interface ISetupData
  {
    /// <summary>
    ///   Invoked before the test is ran to set up any test data
    /// </summary>
    void SetupData();

    /// <summary>
    ///   Invoked after the test is to do any optional cleanup (should be destroyed with test)
    /// </summary>
    void CleanupData();
  }
}