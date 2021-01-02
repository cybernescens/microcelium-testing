using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs
{
  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class GettingViewModelFromControllerAction : SpecsForController<GettingViewModelFromControllerAction.TestController, ViewResult, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    protected override TestController Arrange(Func<TestController> controllerCreator)
    {
      var controller = base.Arrange(controllerCreator);

      var request = controller.HttpContext.Request;
      request.HttpMethod.Returns(Method.Method);

      return controller;
    }

    protected override Expression<Action<TestController>> Action => sut => sut.Get();

    [Test]
    public void ViewModelIsReturned() => Result.Model.Should().NotBeNull();

    public class TestController : Controller
    {
      [HttpGet]
      // ReSharper disable once Mvc.ViewNotResolved
      public ViewResult Get() => View(new {Test = "test"});
    }
  }
}