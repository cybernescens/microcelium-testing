using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.TestConfigFixtures
{
  internal class LoadingProperties
  {
    [Test]
    public void LoadsBoolValueFromSecondResolver() => new Config().BoolValue.Should().BeTrue();

    [Test]
    public void LoadsIntValueFromFirstResolver() => new Config().IntValue.Should().Be(123);

    [Test]
    public void LoadsDefaultDecimalValue() => new Config().DefaultDecimal.Should().Be(1.23M);

    [Test]
    public void ThrowsExceptionIfRequiredValueIsMissing()
    {
      Action act = () => { var foo = new Config().RequiredString; };

      act
        .Should().Throw<Exception>()
        .WithMessage("Missing required configuration for 'd'");
    }

    private class Config : TestConfig
    {
      public Config() : base(
        new LoggerFactory().CreateLogger<Config>(),
        x => {
          var keyValues = new Dictionary<string, string> {{"a", "123"}};
          return keyValues[x];
        }, 
        x => {
          var keyValues = new Dictionary<string, string> {{"a", "987"}, {"b", "true"}};
          return keyValues[x];
        }) { }

      public int IntValue => LoadValue<int>("a");
      public bool BoolValue => LoadValue<bool>("b");
      public decimal DefaultDecimal => LoadValue("c", 1.23M);
      public string RequiredString => LoadValue<string>("d", required: true);
    }
  }
}