using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing;

public static class SafelyTry
{
  [DebuggerNonUserCode]
  public static void Action<TSubject>(TSubject subject, Expression<Action<TSubject>> action, ILogger? log = null)
  {
    try
    {
      log?.LogDebug("Attempting action '{Action}'", action);
      var @delegate = (Delegate)action.Compile();
      @delegate.DynamicInvoke(subject);
    }
    catch (Exception e)
    {
      log?.LogError(e.InnerException ?? e, "Failed to perform action '{Action}'", action);
    }
  }

  [DebuggerNonUserCode]
  public static void Action(Expression<Action> action, ILogger? log = null)
  {
    try
    {
      log?.LogDebug("Attempting action '{Action}'", action);
      var @delegate = (Delegate)action.Compile();
      @delegate.DynamicInvoke();
    }
    catch (Exception e)
    {
      log?.LogError(e.InnerException ?? e, "Failed to perform action '{Action}'", action);
    }
  }

  [DebuggerNonUserCode]
  public static void Dispose(IDisposable? disposable, ILogger? log = null)
  {
    if (disposable == null)
      return;

    Action(disposable, x => x.Dispose(), log);
  }

  [DebuggerNonUserCode]
  public static TValue? Function<TValue>(Expression<Func<TValue>> func, ILogger? log = null)
  {
    try
    {
      log?.LogDebug("Attempting function '{Function}'", func);
      var @delegate = (Delegate)func.Compile(); 
      var o = @delegate.DynamicInvoke();
      return o == null ? default : (TValue)o;
    }
    catch (Exception e)
    {
      log?.LogError(e.InnerException ?? e, "Failed to perform action '{Function}'", func);
    }

    return default;
  }
}