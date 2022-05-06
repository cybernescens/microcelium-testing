using System.Net;
using System.Threading.Tasks;

namespace Microcelium.Testing.Selenium.Authentication;

/// <summary>
/// Does no Cookie persistence
/// </summary>
public class NoOpCookiePersister : ICookiePersister
{
  /// <inheritdoc />
  public Task Persist(CookieContainer container, object? state = null) => Task.CompletedTask;

  /// <inheritdoc />
  public Task<CookieContainer> Retrieve(object? state = null) => Task.FromResult(new CookieContainer());

  public Task Reset() => Task.CompletedTask;

  /// <inheritdoc />
  public bool Initialized => false;
}

/// <summary>
/// NoOp Cookie Persister... does nothing
/// </summary>
public class NoOpCookiePersisterConfig { }