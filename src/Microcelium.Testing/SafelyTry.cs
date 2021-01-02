using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing
{
  public static class SafelyTry
  {
    public static void Action<TSubject>(TSubject subject, Expression<Action<TSubject>> action, ILogger log = null)
    {
      try
      {
        log?.LogDebug("Attempting action '{0}'", action);
        action.Compile()(subject);
      }
      catch (Exception e)
      {
        log?.LogError(e, "Failed to perform action '{0}'", action);
      }
    }

    public static void Action(Expression<Action> action, ILogger log = null)
    {
      try
      {
        log?.LogDebug("Attempting action '{0}'", action);
        action.Compile()();
      }
      catch (Exception e)
      {
        log?.LogError(e, "Failed to perform action '{0}'", action);
      }
    }

    public static void Dispose(IDisposable disposable, ILogger log = null)
    {
      if (disposable == null)
        return;

      Action(disposable, x => x.Dispose(), log);
    }

    public static TValue Function<TValue>(Expression<Func<TValue>> func, ILogger log = null)
    {
      try
      {
        log?.LogDebug("Attempting function '{0}'", func);
        return func.Compile()();
      }
      catch (Exception e)
      {
        log?.LogError(e, "Failed to perform action '{0}'", func);
      }

      return default(TValue);
    }
  }
}