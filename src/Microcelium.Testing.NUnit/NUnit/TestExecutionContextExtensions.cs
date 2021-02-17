using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extension methods for the <see cref="TestExecutionContext"/>
  /// </summary>
  public static class TestExecutionContextExtensions
  {
    /* this is to ensure there is only one */
    private static Lazy<ConcurrentDictionary<string, IDictionary<string, object>>> lazy =
      new Lazy<ConcurrentDictionary<string, IDictionary<string, object>>>(() => 
        new ConcurrentDictionary<string, IDictionary<string, object>>(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static ConcurrentDictionary<string, IDictionary<string, object>> Context = lazy.Value;

    private static string NearestName(ITest currentTest) =>
      currentTest switch
      {
        TestMethod x           => x.FullName,
        Test { Parent: { } } x => NearestName(x.Parent),
        Test _                 => null,
        { }                    => null,
        null                   => null
      };

    public static void SetSuiteProperty<T>(this TestExecutionContext context, string key, T value)
    {
      var name = NearestName(context.CurrentTest);
      if (Context.TryAdd(name, new Dictionary<string, object> { { key, value }}))
        return;

      if (Context.TryGetValue(name, out var dict) && dict.ContainsKey(key))
      {
        dict[key] = value;
        return;
      }
      
      if (dict != null && !dict.ContainsKey(key))
      {
        dict.Add(key, value);
        return;
      }

      var tmp = dict[key];
      if (!ReferenceEquals(tmp, value))
      {
        //warning warning! adding twice!?
        dict[key] = value;
      }
    }

    /// <summary>
    ///   Gets the object stored in a Test's <see cref="IPropertyBag" />
    /// </summary>
    /// <param name="context">the <see cref="TestExecutionContext" /></param>
    /// <param name="key">the key used to store the object in the <see cref="IPropertyBag" /></param>
    /// <returns>the object stored for the key</returns>
    public static object GetSuiteProperty(this TestExecutionContext context, string key)
    {
      var name = NearestName(context.CurrentTest);
      if (Context.TryGetValue(name, out var dict) && dict.ContainsKey(key))
        return dict[key];

      return null;
    }

    public static void ClearSuiteProperties(this TestExecutionContext context)
    {
      var name = NearestName(context.CurrentTest);
      if (Context.TryRemove(name, out var dict))
        dict.Clear();
    }
  }
}