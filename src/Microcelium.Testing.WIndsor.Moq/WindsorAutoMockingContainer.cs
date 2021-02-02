using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Moq;

namespace Microcelium.Testing.Windsor.Moq
{
  /// <inheritdoc cref="IAutoMocker" />
  public class WindsorAutoMockingContainer : WindsorContainer, IAutoMocker
  {
    private MockRepository mocks = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

    public WindsorAutoMockingContainer() { }

    public void Override(MockRepository mocks)
    {
      this.mocks = mocks;
    }

    /// <inheritdoc cref="IAutoMocker.CreateSut{TSut}" />
    public TSut CreateSut<TSut>() where TSut : class
    {
      if (!Kernel.HasComponent(typeof(TSut)))
        Register(Component.For<TSut>().LifestyleTransient().PropertiesIgnore(_ => true));
      
      return Resolve<TSut>();
    }

    /// <inheritdoc cref="IAutoMocker.Mock{TSut}" />
    public TMock Mock<TMock>()
      where TMock : class
    {
      var mock = mocks.Create<TMock>(MockBehavior.Loose).Object;
      Register(Component.For<TMock>().Instance(mock));
      return mock;
    }

    /// <inheritdoc cref="IAutoMocker.RegisterDependency{TSut}" />
    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class =>
      Register(Component.For<TDependency>().Instance(dependency));

    /// <inheritdoc cref="IAutoMocker.RegisterDependency{TSut}" />
    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency =>
      Register(Component.For<TDependency>().ImplementedBy<TImplementation>());

    /// <inheritdoc cref="IAutoMocker.ResolveDependency{TService}" />
    public TService ResolveDependency<TService>() => Resolve<TService>();

    /// <inheritdoc cref="IAutoMocker.TearDown" />
    public void TearDown() => Dispose();
  }
}