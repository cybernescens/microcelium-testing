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
  class HandlingPostOnControllerAction : SpecsForController<HandlingPostOnControllerAction.TestController, ActionResult, MvcAutoMocker<WindsorAutoMockingContainer>>
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

    protected override Expression<Action<TestController>> Action => sut => sut.Post();

    [Test]
    public void CallsPostMethod() => Result.Should().NotBeNull();

    public class TestController : Controller
    {
      [HttpPost]
      public ActionResult Post() => new JsonResult();
    }
  }
}