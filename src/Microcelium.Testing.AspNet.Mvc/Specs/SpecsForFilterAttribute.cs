using System;
using System.Web;
using System.Web.Mvc;

namespace Microcelium.Testing.Specs
{
  public abstract class
    SpecsForFilterAttribute<TFilter, TContext, TAutoMocker> : AutoMockSpecFor<TFilter, TContext, TAutoMocker>
    where TFilter : FilterAttribute
    where TContext : ControllerContext, new()
    where TAutoMocker : IMvcAutoMocker, new()
  {
    protected abstract Action<TFilter, TContext> FilterAction { get; }

    protected override TFilter Arrange(Func<TFilter> attributeCreator)
    {
      RegisterDependency(AutoMocker.CreateHttpContext());
      return attributeCreator();
    }

    protected virtual TContext CreateContext() => new TContext
      {
        HttpContext = ResolveDependency<HttpContextBase>()
      };

    protected override TContext Act(TFilter subject)
    {
      var context = CreateContext();

      FilterAction(subject, context);

      return context;
    }
  }
}