namespace Microcelium.Testing;

public interface IAutoMocker
{
  TSut CreateSut<TSut>() where TSut : class;
  TMock Mock<TMock>() where TMock : class;
  void RegisterDependency<TDependency>(TDependency dependency);
  void RegisterDependency<TDependency, TImplementation>() where TImplementation : TDependency;
  TService ResolveDependency<TService>();
  void TearDown();
}