using System;
using System.Web.Mvc;

namespace Microcelium.Testing.Specs
{
  public abstract class
    SpecsForAuthorizeAttribute<TFilter, TAutoMocker> : SpecsForFilterAttribute<TFilter, AuthorizationContext, TAutoMocker>
    where TFilter : AuthorizeAttribute
    where TAutoMocker : IMvcAutoMocker, new()
  {
    protected override Action<TFilter, AuthorizationContext> FilterAction => (f, c) => f.OnAuthorization(c);

    protected override TFilter Arrange(Func<TFilter> attributeCreator)
    {
      RegisterDependency(AutoMocker.CreateActionDescriptor());
      return base.Arrange(attributeCreator);
    }

    protected override AuthorizationContext CreateContext()
    {
      var authorizationContext = base.CreateContext();
      authorizationContext.ActionDescriptor = ResolveDependency<ActionDescriptor>();
      return authorizationContext;
    }
  }
}