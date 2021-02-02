using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing
{
  /// <summary>
  /// Utility to safely perform some actions without throwing an exception
  /// </summary>
  public static class SafelyTry
  {
    /// <summary>
    /// Safely executes an action
    /// </summary>
    /// <typeparam name="TSubject">the type of the <paramref name="subject"/></typeparam>
    /// <param name="subject">the subject to be passed to the <paramref name="action"/></param>
    /// <param name="action">the action to perform</param>
    /// <param name="log">log to be logged to</param>
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

    /// <summary>
    /// Safely executes an action
    /// </summary>
    /// <param name="action">the action to perform</param>
    /// <param name="log">log to be logged to</param>
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

    /// <summary>
    /// Safely executes a dispose
    /// </summary>
    /// <param name="disposable">the disposal</param>
    /// <param name="log">log to be logged to</param>
    public static void Dispose(IDisposable disposable, ILogger log = null)
    {
      if (disposable == null)
        return;

      Action(disposable, x => x.Dispose(), log);
    }

    /// <summary>
    /// Safely executes a function
    /// </summary>
    /// <typeparam name="TValue">the type of the return value of <paramref name="func"/></typeparam>
    /// <param name="func">the function to execute</param>
    /// <param name="log">log to be logged to</param>
    /// <returns>the result of the <paramref name="func"/></returns>
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