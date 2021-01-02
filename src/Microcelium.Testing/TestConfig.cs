using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing
{
  public abstract class TestConfig
  {
    private readonly ILogger log;

    protected TestConfig(ILogger log, params Func<string, string>[] propertyResolvers)
    {
      this.log = log;
      PropertyResolvers = propertyResolvers;
    }

    protected Func<string, string>[] PropertyResolvers { get; set; }

    protected TValue LoadValue<TValue>(string key, TValue defaultValue = default(TValue), bool required = false, Func<string, TValue> converter = null)
    {
      log.LogInformation($"Attempting to load property value for '{key}'");

      var value = AttemptToResolveValue(key);

      if (value == null)
        return ReturnDefaultIfNotRequired(key, defaultValue, required);

      log.LogInformation($"Found property value for '{key}': '{value}''");

      return converter != null ? converter(value) : (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFromString(value);
    }

    private TValue ReturnDefaultIfNotRequired<TValue>(string key, TValue defaultValue, bool required)
    {
      if (required)
        throw new Exception($"Missing required configuration for '{key}'");

      log.LogInformation($"No property collections contained a value for '{key}', returning default value '{defaultValue}'");

      return defaultValue;
    }

    private string AttemptToResolveValue(string key)
    {
      for(var i = 0; i < PropertyResolvers.Length; i++)
      {
        var resolver = PropertyResolvers[i];
        var result = SafelyTry.Function(() => resolver(key));
        if (result == null)
          continue;

        log.LogInformation($"Found value @ PropertyResolver index {i}");
        return result;
      }

      return null;
    }
  }
}