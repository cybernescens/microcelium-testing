using System;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Mvc;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs
{
  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class HandlingPostWithNewExpressionInControllerAction : SpecsForController<HandlingPostWithNewExpressionInControllerAction.TestController, JsonResult, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    protected override HttpMethod Method => HttpMethod.Post;

    protected override TestController Arrange(Func<TestController> controllerCreator)
    {
      var controller = base.Arrange(controllerCreator);

      var request = controller.HttpContext.Request;

      request.HttpMethod.Returns(Method.Method);
      request.Headers.Returns(new NameValueCollection());
      request.Form.Returns(new NameValueCollection());
      request.QueryString.Returns(new NameValueCollection());

      return controller;
    }

    protected override Expression<Action<TestController>> Action => sut => sut.Post(new Model());

    [Test]
    public void CallsPostMethod() => Result.Should().NotBeNull();

    [Test]
    public void PostedModelIsAsExpected() => Result.Data.Should().BeOfType<Model>();

    public class TestController : Controller
    {
      [HttpPost]
      public ActionResult Post(Model model) => new JsonResult{ Data = model };
    }

    public class Model {}
  }

  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class HandlingControllerActionWhichTakesAStruct : SpecsForController<HandlingControllerActionWhichTakesAStruct.TestController, JsonResult, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    protected override HttpMethod Method => HttpMethod.Post;

    protected override TestController Arrange(Func<TestController> controllerCreator)
    {
      var controller = base.Arrange(controllerCreator);

      var request = controller.HttpContext.Request;

      request.HttpMethod.Returns(Method.Method);
      request.Headers.Returns(new NameValueCollection());
      request.Form.Returns(new NameValueCollection());
      request.QueryString.Returns(new NameValueCollection());

      return controller;
    }

    protected override Expression<Action<TestController>> Action => sut => sut.Post(new Model());

    [Test]
    public void CallsPostMethod() => Result.Should().NotBeNull();

    [Test]
    public void PostedModelIsAsExpected() => Result.Data.Should().BeOfType<Model>();

    public class TestController : Controller
    {
      [HttpPost]
      public ActionResult Post(Model model) => new JsonResult { Data = model };
    }

    public struct Model { }
  }
}