using Ninject;
using Ninject.MockingKernel.NSubstitute;
using NSubstitute;

namespace Microcelium.Testing.Ninject.NSubstitute
{
  public class NinjectAutoMockingContainer : IAutoMocker
  {
    private readonly NSubstituteMockingKernel kernel = new NSubstituteMockingKernel();

    public TSut CreateSut<TSut>() where TSut : class => kernel.Get<TSut>();

    public TMock Mock<TMock>() where TMock : class => Substitute.For<TMock>();

    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class
      => kernel.Bind<TDependency>().ToConstant(dependency);

    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency
      => kernel.Bind<TDependency>().To<TImplementation>();

    public TService ResolveDependency<TService>() => kernel.Get<TService>();

    public void TearDown() => kernel.Dispose();
  }
}