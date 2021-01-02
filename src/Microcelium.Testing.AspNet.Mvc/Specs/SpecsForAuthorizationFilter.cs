using System;
using System.Web;
using System.Web.Mvc;

namespace Microcelium.Testing.Specs
{
  public abstract class SpecsForAuthorizationFilter<TFilter, TAutoMocker> : AutoMockSpecFor<TFilter, AuthorizationContext, TAutoMocker>
    where TFilter : class, IAuthorizationFilter
    where TAutoMocker : IMvcAutoMocker, new()
  {
    protected override TFilter Arrange(Func<TFilter> attributeCreator)
    {
      RegisterDependency(AutoMocker.CreateHttpContext());
      return attributeCreator();
    }

    protected virtual AuthorizationContext CreateContext() => new AuthorizationContext
      {
        HttpContext = ResolveDependency<HttpContextBase>()
      };

    protected override AuthorizationContext Act(TFilter subject)
    {
      var context = CreateContext();
      subject.OnAuthorization(context);
      return context;
    }
  }
}