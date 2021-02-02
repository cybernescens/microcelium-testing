﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using NSubstitute;

namespace Microcelium.Testing
{
  /// <inheritdoc cref="IAutoMocker" />
  public class WindsorAutoMockingContainer : WindsorContainer, IAutoMocker
  {
    /// <summary>
    /// Instantiates a WindsorAutoMockingContainer
    /// </summary>
    public WindsorAutoMockingContainer()
    {
      Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyComponentAutoMocker>());
    }

    /// <inheritdoc cref="IAutoMocker.CreateSut{TSut}" />
    public TSut CreateSut<TSut>()
      where TSut : class
    {
      if (!Kernel.HasComponent(typeof(TSut)))
        Register(Component.For<TSut>().LifestyleTransient().PropertiesIgnore(_ => true));

      return Resolve<TSut>();
    }

    /// <inheritdoc cref="IAutoMocker.Mock{TSut}" />
    public TMock Mock<TMock>()
      where TMock : class =>
      Substitute.For<TMock>();

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