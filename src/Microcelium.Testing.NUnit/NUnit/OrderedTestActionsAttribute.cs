using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit
{
  public class OrderedTestActionsAttribute : Attribute, ITestAction
  {
    private OrderedTestAction[] actions;

    public void BeforeTest(ITest test)
    {
      if (test.Fixture == null)
        return;

      actions = test.Fixture
        .GetType()
        .GetCustomAttributes<OrderedTestAction>()
        .OrderBy(x => x.Order)
        .ToArray();

      var invalids = actions
        .Select((x, i) => new { Expected = i, Action = x })
        .Where(x => x.Expected != x.Action.Order)
        .ToArray();

      if (invalids.Length > 0)
      {
        var agg = invalids
          .Select(x => $"Expected Index: `{x.Expected}`, Action: `{x.Action.Order}`, Name: `{x.Action.GetType().Name}`")
          .Aggregate("\r\n\t", (acc, x) => $"{acc}\r\n\t{x}");

        throw new Exception(
          "Orders must be exact when using IOrderedTestAction. " +
          "You have the following configuration issues: " + agg);
      }

      for(var i = 0; i < actions.Length; i++)
      {
        var index = i + 1;
        try
        {
          actions[i].BeforeTest(test);
        }
        catch(Exception e)
        {
          var msg = $"Exception occurred executing IOrderedTestAction.BeforeTest number: `{index}`";
          throw new InvalidOperationException(msg, e);
        }
      }
    }

    public void AfterTest(ITest test)
    {
      for (var i = 0; i < actions.Length; i++)
      {
        var index = i + 1;
        try
        {
          actions[i].AfterTest(test);
        }
        catch (Exception e)
        {
          var msg = $"Exception occurred executing IOrderedTestAction.AfterTest number: `{index}`";
          throw new InvalidOperationException(msg, e);
        }
      }
    }

    public ActionTargets Targets { get; } = ActionTargets.Test;
  }

  public interface IOrderedTestAction
  {
    public int Order { get; }
    public void BeforeTest(ITest test);
    public void AfterTest(ITest test);
  }

  public abstract class OrderedTestAction : Attribute, IOrderedTestAction
  {
    public abstract int Order { get; }
    public abstract void BeforeTest(ITest test);
    public abstract void AfterTest(ITest test);
  }
}