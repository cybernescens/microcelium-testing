﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using NSubstitute;

namespace Microcelium.Testing
{
  public class WindsorAutoMockingContainer : WindsorContainer, IAutoMocker
  {
    public WindsorAutoMockingContainer()
    {
      Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyComponentAutoMocker>());
    }

    public TSut CreateSut<TSut>() where TSut : class
    {
      if (!Kernel.HasComponent(typeof(TSut)))
        Register(Component.For<TSut>().LifestyleTransient().PropertiesIgnore(propertyInfo => true));

      return Resolve<TSut>();
    }

    public TMock Mock<TMock>()
      where TMock : class
      => Substitute.For<TMock>();

    public void RegisterDependency<TDependency>(TDependency dependency)
      where TDependency : class
      => Register(Component.For<TDependency>().Instance(dependency));

    public void RegisterDependency<TDependency, TImplementation>()
      where TDependency : class
      where TImplementation : TDependency
      => Register(Component.For<TDependency>().ImplementedBy<TImplementation>());

    public TService ResolveDependency<TService>() => Resolve<TService>();

    public void TearDown() => Dispose();
  }
}