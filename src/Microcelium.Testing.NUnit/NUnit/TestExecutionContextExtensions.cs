using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microcelium.Testing.NUnit
{
  /// <summary>
  /// Extension methods for the <see cref="TestExecutionContext"/>
  /// </summary>
  public static class TestExecutionContextExtensions
  {
    /// <summary>
    /// Gets the object stored in a Test's <see cref="IPropertyBag"/>
    /// </summary>
    /// <typeparam name="FixtureT">the originating fixture's type</typeparam>
    /// <param name="context">the <see cref="TestExecutionContext"/></param>
    /// <param name="key">the key used to store the object in the <see cref="IPropertyBag"/></param>
    /// <returns>the object stored for the key</returns>
    public static object GetSuiteProperty<FixtureT>(this TestExecutionContext context, string key)
    {
      object GetSuiteProperty(ITest currentTest)
      {
        return currentTest switch {
          Test x when x.TypeInfo?.Type?.GetInterfaces().Any(i => i == typeof(FixtureT)) ?? false => x.Properties.Get(key),
          Test {Parent: { }} x => GetSuiteProperty(x.Parent),
          Test _ => null,
          { } => null,
          null => null
        };
      }

      return GetSuiteProperty(context.CurrentTest);
    }
  }
}