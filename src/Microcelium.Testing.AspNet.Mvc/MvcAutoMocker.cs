using System.Web;
using System.Web.Mvc;

namespace Microcelium.Testing
{
  public class MvcAutoMocker<TAutoMocker> : IMvcAutoMocker where TAutoMocker : IAutoMocker, new()
  {
    private readonly TAutoMocker autoMocker = new TAutoMocker();
    public TSut CreateSut<TSut>() where TSut : class => autoMocker.CreateSut<TSut>();

    public TMock Mock<TMock>() where TMock : class => autoMocker.Mock<TMock>();

    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class
      => autoMocker.RegisterDependency(dependency);

    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency
      => autoMocker.RegisterDependency<TDependency, TImplementation>();

    public TService ResolveDependency<TService>() => autoMocker.ResolveDependency<TService>();

    public void TearDown() => autoMocker.TearDown();

    public HttpContextBase CreateHttpContext() => autoMocker.Mock<HttpContextBase>();
    public ActionDescriptor CreateActionDescriptor() => autoMocker.Mock<ActionDescriptor>();
  }
}