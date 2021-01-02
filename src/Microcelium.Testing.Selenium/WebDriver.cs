using System;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  /// Configuration initialization point
  /// </summary>
  public static class WebDriver
  {
    /// <summary>
    /// Instantiates and configures a <see cref="WebDriverConfigBuilder"/> object that ultimately builds a <see cref="WebDriverConfig"/>
    /// </summary>
    /// <param name="configure">the configuration action</param>
    /// <param name="log"></param>
    /// <returns>a partially configured <see cref="WebDriverConfigBuilder"/></returns>
    public static WebDriverConfigBuilder Configure(Func<WebDriverConfigBuilder, WebDriverConfigBuilder> configure, ILogger log)
    {
      var builder = new WebDriverConfigBuilder(log);
      configure(builder);
      return builder;
    }
  }
}