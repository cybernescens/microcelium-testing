﻿using Ninject;
using Ninject.MockingKernel.NSubstitute;
using NSubstitute;

namespace Microcelium.Testing
{
  /// <inheritdoc cref="IAutoMocker" />
  public class NinjectAutoMockingContainer : IAutoMocker
  {
    private readonly NSubstituteMockingKernel kernel = new NSubstituteMockingKernel();

    /// <inheritdoc cref="IAutoMocker.CreateSut{TSut}" />
    public TSut CreateSut<TSut>() where TSut : class => kernel.Get<TSut>();

    /// <inheritdoc cref="IAutoMocker.Mock{TSut}" />
    public TMock Mock<TMock>() where TMock : class => Substitute.For<TMock>();

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