using System.Web;
using System.Web.Mvc;

namespace Microcelium.Testing
{
  /// <summary>
  /// Automocking for MVC
  /// </summary>
  /// <typeparam name="TAutoMocker">the type of Automocker</typeparam>
  public class MvcAutoMocker<TAutoMocker> : IMvcAutoMocker where TAutoMocker : IAutoMocker, new()
  {
    private readonly TAutoMocker autoMocker = new TAutoMocker();

    /// <inheritdoc />
    public TSut CreateSut<TSut>() where TSut : class => autoMocker.CreateSut<TSut>();

    /// <inheritdoc />
    public TMock Mock<TMock>() where TMock : class => autoMocker.Mock<TMock>();

    /// <inheritdoc />
    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class
      => autoMocker.RegisterDependency(dependency);

    /// <inheritdoc />
    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency
      => autoMocker.RegisterDependency<TDependency, TImplementation>();

    /// <inheritdoc />
    public TService ResolveDependency<TService>() => autoMocker.ResolveDependency<TService>();

    /// <inheritdoc />
    public void TearDown() => autoMocker.TearDown();

    /// <summary>
    /// Creates the <see cref="HttpContextBase"/>
    /// </summary>
    /// <returns></returns>
    public HttpContextBase CreateHttpContext() => autoMocker.Mock<HttpContextBase>();

    /// <summary>
    /// Create the <see cref="ActionDescriptor"/>
    /// </summary>
    /// <returns></returns>
    public ActionDescriptor CreateActionDescriptor() => autoMocker.Mock<ActionDescriptor>();
  }
}