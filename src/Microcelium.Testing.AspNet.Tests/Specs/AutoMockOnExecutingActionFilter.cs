using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs
{
  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class AutoMockOnExecutingActionFilter : SpecsForOnActionExecutingActionFilterAttribute<ActionFilterAttribute, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    private ActionFilterAttribute sut;

    protected override ActionFilterAttribute CreateSubject() => sut = Substitute.For<ActionFilterAttribute>();

    [Test]
    public void ActonAttributeOnExecutingActionCalled() => sut.Received().OnActionExecuting(Result);
  }
}