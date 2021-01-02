using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.NUnit
{
  public class NUnitParameterPropertyResolver
  {
    private readonly ILogger log;

    public NUnitParameterPropertyResolver(ILogger log) { this.log = log; }

    public string Resolve(string key)
    {
      log.LogInformation($"Attempted to load key '{key}' from NUnit parameters");

      var property = TestContext.Parameters[key];

      if (property == null) log.LogInformation($"NUnit parameters does not contain value for '{key}'");

      return property;
    }
  }
}