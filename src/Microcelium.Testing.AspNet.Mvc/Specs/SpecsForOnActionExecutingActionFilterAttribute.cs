using System;
using System.Web.Mvc;

namespace Microcelium.Testing.Specs
{
  public abstract class
    SpecsForOnActionExecutingActionFilterAttribute<TFilter, TAutoMocker> : SpecsForFilterAttribute<TFilter, ActionExecutingContext, TAutoMocker>
    where TFilter : ActionFilterAttribute
    where TAutoMocker : IMvcAutoMocker, new()
  {
    protected override Action<TFilter, ActionExecutingContext> FilterAction => (f, c) => f.OnActionExecuting(c);
  }
}