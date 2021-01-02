using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Specs
{
  /// <summary>
  /// Decorates a <see cref="SpecsFor{TSut,TResult}"/> or <see cref="AsyncSpecsFor{TSut,TResult}"/>
  /// unit test so nunit can automatically execute it
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class SpecAttribute : Attribute, ITestAction
  {
    private static readonly Type SpecsType = typeof(SpecsFor<,>);
    private static readonly Type AsyncSpecsType = typeof(AsyncSpecsFor<,>);

    public void BeforeTest(ITest test)
    {
      var fixtureType = test.Fixture.GetType();

      if (IsSubclassOfGeneric(SpecsType, fixtureType))
      {
        fixtureType.GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(test.Fixture, new object[0]);
        return;
      }

      if (IsSubclassOfGeneric(AsyncSpecsType, fixtureType))
      {
        var task = (Task) fixtureType.GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(test.Fixture, new object[0]);
        task.GetAwaiter().GetResult();
        return;
      }

      throw new Exception($"The attribute '{GetType()}' is only intended for fixtures that extend the '{SpecsType.FullName}' or '{AsyncSpecsType.FullName}' class");
    }

    private static bool IsSubclassOfGeneric(Type generic, Type type) 
      => type != null
         && type != typeof(object)
         && (generic ==
              (type.IsGenericType
                ? type.GetGenericTypeDefinition() 
                : type)
             || IsSubclassOfGeneric(generic, type.BaseType));

    public void AfterTest(ITest test) { }

    public ActionTargets Targets => ActionTargets.Suite;
  }
}