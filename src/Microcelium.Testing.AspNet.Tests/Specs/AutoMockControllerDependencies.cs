using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Microcelium.Testing.Specs
{
  [Spec]
  [Parallelizable(ParallelScope.Children)]
  class AutoMockControllerDependencies : SpecsForController<AutoMockControllerDependencies.TestController, JsonResult, MvcAutoMocker<WindsorAutoMockingContainer>>
  {
    protected override TestController Arrange(Func<TestController> controllerCreator)
    {
      var injectedDependency = Substitute.For<IFakeDependencyInjected>();
      injectedDependency.CreateInteger().Returns(3);

      RegisterDependency(injectedDependency);
      RegisterDependency<IImplementedDependency, ImplementedDependency>();

      ResolveDependency<IFakeDependencyMocked>().CreateInteger().Returns(9);

      return base.Arrange(controllerCreator);
    }

    protected override Expression<Action<TestController>> Action => sut => sut.GetJson();

    [Test]
    public void JsonResultContainsValueFromMockedDependency()
      => Result.Data.Should().BeOfType<Data>().Which.MockedDependencyResult.Should().Be(9);

    [Test]
    public void JsonResultContainsValueFromInjectedDependency()
      => Result.Data.Should().BeOfType<Data>().Which.InjectedDependencyResult.Should().Be(3);

    [Test]
    public void JsonResultContainsValueFromImplementedDependency()
      => Result.Data.Should().BeOfType<Data>().Which.ImplementedDependencyResult.Should().Be("Hello world");
    
    [Test]
    public void JsonResultContainsValueFromInstanceDependency()
      => Result.Data.Should().BeOfType<Data>().Which.InstanceDependencyResult.Should().Be("Goodbye world");

    public class TestController : Controller
    {
      private readonly IFakeDependencyInjected dependencyInjected;
      private readonly IImplementedDependency implementedDependency;
      private readonly InstanceDependency instanceDependency;
      private readonly IFakeDependencyMocked dependencyMocked;

      public TestController(
        IFakeDependencyMocked dependencyMocked,
        IFakeDependencyInjected dependencyInjected,
        IImplementedDependency implementedDependency,
        InstanceDependency instanceDependency)
      {
        this.dependencyInjected = dependencyInjected;
        this.implementedDependency = implementedDependency;
        this.instanceDependency = instanceDependency;
        this.dependencyMocked = dependencyMocked;
      }
      public JsonResult GetJson() => new JsonResult
      {
        Data = new Data
        {
          MockedDependencyResult = dependencyMocked.CreateInteger(),
          InjectedDependencyResult = dependencyInjected.CreateInteger(),
          ImplementedDependencyResult = implementedDependency.CreateString(),
          InstanceDependencyResult = instanceDependency.CreateString()
        }
      };
    }

    public interface IFakeDependencyMocked
    {
      int CreateInteger();
    }

    public interface IFakeDependencyInjected
    {
      int CreateInteger();
    }

    public interface IImplementedDependency
    {
      string CreateString();
    }

    class ImplementedDependency : IImplementedDependency
    {
      public string CreateString() => "Hello world";
    }

    internal class InstanceDependency
    {
      public string CreateString() => "Goodbye world";
    }

    public class Data
    {
      public int MockedDependencyResult { get; set; }
      public int InjectedDependencyResult { get; set; }
      public string ImplementedDependencyResult { get; set; }
      public string InstanceDependencyResult { get; set; }
    }
  }
}