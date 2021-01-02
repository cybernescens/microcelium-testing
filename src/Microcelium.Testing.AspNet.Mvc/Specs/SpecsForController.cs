using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Microcelium.Testing.Specs
{
  public abstract class SpecsForController<TSut, TResult, TAutoMocker> : AutoMockSpecFor<TSut, TResult, TAutoMocker> 
    where TSut : Controller
    where TResult : ActionResult
    where TAutoMocker : IMvcAutoMocker, new()
  {
    protected HttpContextBase HttpContext => Subject.ControllerContext.HttpContext;

    protected virtual HttpMethod Method => HttpMethod.Get;

    protected abstract Expression<Action<TSut>> Action { get; }

    protected override TSut Arrange(Func<TSut> controllerCreator)
    {
      var httpContextBase = AutoMocker.CreateHttpContext();

      var controllerContext = new ControllerContext
        {
          RouteData = new RouteData(),
          HttpContext = httpContextBase
        };

      System.Web.HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://tempuri.org", "")
          {
            RequestContext = new RequestContext(controllerContext.HttpContext, controllerContext.RouteData),
            Browser = new HttpBrowserCapabilities
              {
                Capabilities =
                  new Dictionary<string, string> {{"supportsEmptyStringInCookieValue", "false"}, {"cookies", "false"}}
              }
          },
        new HttpResponse(new StringWriter()));

      var controller = base.Arrange(controllerCreator);

      controllerContext.Controller = controller;
      controller.ControllerContext = controllerContext;

      return controller;
    }

    protected override TResult Act(TSut subject)
    {
      var actionInvoker = new ControllerSpecActionInvoker<TResult>(Action.Body);
      var methodName = ExtractActionName(Action.Body);
      actionInvoker.InvokeAction(Subject.ControllerContext, methodName);

      return actionInvoker.Result;
    }

    private static string ExtractActionName(Expression body)
    {
      switch (body)
      {
        case MethodCallExpression e:
          return e.Method.Name;
        case UnaryExpression e:
          return ExtractActionName(e.Operand);
        default:
          throw new Exception("Cannot determine action from action");
      }
    }

    private class ControllerSpecActionInvoker<TActionResult> : ControllerActionInvoker where TActionResult : ActionResult
    {
      private readonly Expression body;

      public ControllerSpecActionInvoker(Expression body)
      {
        this.body = body;
      }

      public TActionResult Result { get; private set; }

      protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        => Result = actionResult as TActionResult;

      protected override IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        => body is MethodCallExpression methodCall
          ? methodCall.Method.GetParameters()
            .Zip(
              methodCall.Arguments.Select(GetArgumentAsConstant),
              (param, arg) => new {param.Name, Value = ChangeType(arg, param.ParameterType)})
            .ToDictionary(item => item.Name, item => item.Value)
          : base.GetParameterValues(controllerContext, actionDescriptor);

      private static object GetArgumentAsConstant(Expression exp)
      {
        object GetValue(Expression expression) =>
          Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile()();

        switch (exp)
        {
          case ConstantExpression constExp:
            return constExp.Value;
          case NewExpression newExp:
            return GetValue(newExp);
          case MemberInitExpression newExp:
            return GetValue(newExp);
          case MemberExpression newExp:
            return GetValue(newExp);
          case UnaryExpression uranExp:
            return GetArgumentAsConstant(uranExp.Operand);
        }

        throw new NotSupportedException($"Cannot handle expression of type '{exp.GetType()}'");
      }

      private static object ChangeType(object value, Type conversion)
        => !conversion.IsGenericType || conversion.GetGenericTypeDefinition() != typeof(Nullable<>)
          ? Convert.ChangeType(value, conversion)
          : value == null
            ? null
            : Convert.ChangeType(value, Nullable.GetUnderlyingType(conversion));
    }
  }
}