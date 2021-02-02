using Moq;
using Ninject;

namespace Microcelium.Testing
{
  /// <inheritdoc cref="IAutoMocker" />
  public class NinjectAutoMockingContainer : IAutoMocker
  {
    private readonly MockRepository mocks;
    private readonly IKernel kernel;

    /// <summary>
    /// Instantiates a <see cref="NinjectAutoMockingContainer"/>
    /// </summary>
    /// <param name="kernel">an already existing <see cref="IKernel"/></param>
    /// <param name="mocks">an already existing <see cref="MockRepository"/></param>
    public NinjectAutoMockingContainer(IKernel kernel = null, MockRepository mocks = null)
    {
      this.mocks = mocks;
      this.kernel = kernel ?? new StandardKernel();
    }

    /// <inheritdoc cref="IAutoMocker.CreateSut{TSut}" />
    public TSut CreateSut<TSut>()
      where TSut : class
    {
      var mock = mocks.Create<TSut>(MockBehavior.Strict);
      kernel.Bind<TSut>().ToConstant(mock.Object);
      return kernel.Get<TSut>();
    }

    /// <inheritdoc cref="IAutoMocker.Mock{TSut}" />
    public TMock Mock<TMock>()
      where TMock : class
    {
      var mock = mocks.Create<TMock>(MockBehavior.Loose);
      kernel.Bind<TMock>().ToConstant(mock.Object);
      return kernel.Get<TMock>();
    }

    /// <inheritdoc cref="IAutoMocker.RegisterDependency{TSut}" />
    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class => 
      kernel.Bind<TDependency>().ToConstant(dependency);

    /// <inheritdoc cref="IAutoMocker.RegisterDependency{TSut}" />
    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency =>
      kernel.Bind<TDependency>().To<TImplementation>();

    /// <inheritdoc cref="IAutoMocker.ResolveDependency{TService}" />
    public TService ResolveDependency<TService>() => kernel.Get<TService>();

    /// <inheritdoc cref="IAutoMocker.TearDown" />
    public void TearDown() => kernel.Dispose();
  }
}