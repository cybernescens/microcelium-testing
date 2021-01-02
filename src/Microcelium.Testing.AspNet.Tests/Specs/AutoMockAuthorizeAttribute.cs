using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs
{
  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class AutoMockAuthorizeAttribute : SpecsForAuthorizeAttribute<AuthorizeAttribute, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    private AuthorizeAttribute sut;

    protected override AuthorizeAttribute CreateSubject() => sut = Substitute.For<AuthorizeAttribute>();

    [Test]
    public void ActonAttributeOnExecutingActionCalled() => sut.Received().OnAuthorization(Result);
  }
}