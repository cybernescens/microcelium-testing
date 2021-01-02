using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.TestConfigFixtures
{
  class LoadingPropertiesFromANameValueCollection
  {
    [Test]
    public void LoadsIntValueFromCollection() => new Config().IntValue.Should().Be(123);

    [Test]
    public void LoadsDefaultValue() => new Config().IntDefaultValue.Should().Be(321);

    class Config : TestConfig
    {
      private static readonly NameValueCollectionPropertyResolver propertyResolver =
        new NameValueCollectionPropertyResolver(
          new NameValueCollection { { "a", "123" } }, 
          new LoggerFactory().CreateLogger<NameValueCollectionPropertyResolver>());

      public Config() : base(new LoggerFactory().CreateLogger<Config>(), propertyResolver.Resolve) { }
      public int IntValue => LoadValue<int>("a");
      public int IntDefaultValue => LoadValue<int>("b", 321);
    }
  }
}