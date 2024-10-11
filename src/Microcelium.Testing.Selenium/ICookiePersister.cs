using System.Net;
using System.Threading.Tasks;

namespace Microcelium.Testing.Selenium;

/// <summary>
/// Persists cookie to span multi test or even test-runs
/// </summary>
public interface ICookiePersister
{
  /// <summary>
  /// Persists the Cookies
  /// </summary>
  /// <param name="container">the <see cref="CookieContainer"/></param>
  /// <param name="state">arbitrary implementation specific state</param>
  /// <returns></returns>
  Task Persist(CookieContainer container, object? state = null);

  /// <summary>
  /// Retrieves Cookies
  /// </summary>
  /// <param name="state">arbitrary implementation specific state</param>
  /// <returns></returns>
  Task<CookieContainer> Retrieve(object? state = null);

  /// <summary>
  /// Has the persister been initialized from its perspective
  /// </summary>
  /// <value></value>
  bool Initialized { get; }
}