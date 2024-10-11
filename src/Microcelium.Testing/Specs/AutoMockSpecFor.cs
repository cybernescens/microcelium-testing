namespace Microcelium.Testing.Specs;

/// <summary>
///   Adds Automocking to the Arrange - Act - Assert workflow provided by <see cref="SpecsFor{TSut,TResult}" />
/// </summary>
/// <typeparam name="TSut">the type of System Under Test</typeparam>
/// <typeparam name="TResult">the type of result Result</typeparam>
/// <typeparam name="TAutoMocker">the type of Automocker (e.g. NInject, Windsor)</typeparam>
public abstract class AutoMockSpecFor<TSut, TResult, TAutoMocker> : SpecsFor<TSut, TResult>
  where TSut : class
  where TAutoMocker : IAutoMocker, new()
{
  /// <summary>
  ///   Instantiates the <see cref="AutoMocker" />
  /// </summary>
  protected AutoMockSpecFor() { AutoMocker = new TAutoMocker(); }

  /// <summary>
  ///   The instantiated AutoMocking framework object
  /// </summary>
  protected TAutoMocker AutoMocker { get; }

  /// <inheritdoc />
  protected override TSut CreateSubject() => AutoMocker.CreateSut<TSut>();

  /// <summary>
  ///   Uses the <see cref="AutoMocker" /> to obtain an instantiated <typeparamref name="TService" />.
  ///   Dependency is configured during the <see cref="SpecsFor{TSut,TResult}.Arrange" /> step.
  /// </summary>
  /// <typeparam name="TService">the service/support object required by the System Under Test</typeparam>
  /// <returns></returns>
  protected TService ResolveDependency<TService>() => AutoMocker.ResolveDependency<TService>();

  /// <summary>
  ///   Registers a custom object for a particular service.
  ///   E.g. we might want basic and non-default Mocking and need to perform much more
  ///   elaborate setup of or mocked objects
  ///   Dependency is configured during the <see cref="SpecsFor{TSut,TResult}.Arrange" /> step.
  /// </summary>
  /// <typeparam name="TService">the service/support object needed</typeparam>
  /// <param name="dependency">the service implementation</param>
  protected void RegisterDependency<TService>(TService dependency)
    where TService : class =>
    AutoMocker.RegisterDependency(dependency);

  /// <summary>
  ///   Registers the type of implementation for a particular service.
  ///   Dependency is configured during the <see cref="SpecsFor{TSut,TResult}.Arrange" /> step.
  /// </summary>
  /// <typeparam name="TService"></typeparam>
  /// <typeparam name="TImplementation"></typeparam>
  protected void RegisterDependency<TService, TImplementation>()
    where TService : class
    where TImplementation : TService =>
    AutoMocker.RegisterDependency<TService, TImplementation>();

  /// <inheritdoc />
  protected override void TearDown() => AutoMocker.TearDown();
}