using System.Collections.Specialized;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing
{
  public class NameValueCollectionPropertyResolver
  {
    private readonly NameValueCollection properties;
    private readonly ILogger log;

    public NameValueCollectionPropertyResolver(NameValueCollection properties, ILogger log)
    {
      this.properties = properties;
      this.log = log;
    }

    public string Resolve(string key)
    {
      log.LogInformation($"Attempted to load key '{key}' from Name Value collection");

      var property = properties[key];

      if (property == null)
      {
        log.LogInformation($"Name Value collection does not contain value for '{key}'");
      }
      return property;
    }
  }
}