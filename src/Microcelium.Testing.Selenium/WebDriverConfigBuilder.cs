using System;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  ///   Static and global selenium configuration; write-only
  /// </summary>
  public class WebDriverConfigBuilder : TestConfig
  {
    private Action<ChromeOptions> options = null;
    private DirectoryInfo downloadDirectory;

    private static readonly Lazy<IConfiguration> ConfigFile = new Lazy<IConfiguration>(
      () => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build());

    internal WebDriverConfigBuilder(ILogger log) : base(log) {
    }

    /// <summary>
    /// Sets default Chrome options
    /// </summary>
    /// <returns>a reference to self</returns>
    public WebDriverConfigBuilder WithDefaultOptions()
    {
      options =
        chromeOpts =>
        {
          chromeOpts.AddUserProfilePreference("download.prompt_for_download", false);
          chromeOpts.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
        };

      PropertyResolvers = new Func<string, string>[]
        {
          key => Environment.GetEnvironmentVariable($"{key}"),
          key => ConfigFile.Value[$"settings:{key}"]
        };
      
      return this;
    }

    /// <summary>
    /// Sets Chrome options
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public WebDriverConfigBuilder WithOptions(Action<ChromeOptions> options)
    {
      this.options = options;
      return this;
    }

    /// <summary>
    /// Adds (in priority order), property resolvers
    /// </summary>
    /// <param name="propertyResolvers"></param>
    /// <returns></returns>
    public WebDriverConfigBuilder Providers(params Func<string, string>[] propertyResolvers)
    {
      PropertyResolvers = propertyResolvers;
      return this;
    }

    /// <summary>
    /// Specifies the download directory for any tests that may require downloading files
    /// </summary>
    /// <param name="downloadDirectory">the target directory</param>
    /// <returns></returns>
    public WebDriverConfigBuilder DownloadDirectory(DirectoryInfo downloadDirectory)
    {
      this.downloadDirectory = downloadDirectory;
      return this;
    }

    /// <summary>
    /// Buids the <see cref="IWebDriverConfig"/>
    /// </summary>
    /// <returns></returns>
    public IWebDriverConfig Build()
    {
      var driverConfig = new WebDriverConfig();
      driverConfig.BrowserType = LoadValue("webdriver.browser.type", typeof(ChromeDriver).FullName);
      driverConfig.BrowserSize = LoadValue("webdriver.browser.size", new Size(1280, 1024));
      driverConfig.RunHeadless = LoadValue("webdriver.browser.runheadless", true);
      driverConfig.PageLoadTimeout = LoadValue("webdriver.timeout.pageload", TimeSpan.FromSeconds(10)); ;
      driverConfig.ImplicitTimeout = LoadValue("webdriver.timeout.implicitwait", TimeSpan.FromSeconds(0)); ;
      driverConfig.BrowserTimeout = LoadValue("webdriver.timeout.browser", TimeSpan.FromSeconds(60)); ;
      driverConfig.ChromeOptions = CreateChromeOptions(driverConfig);
      driverConfig.BaseUrl = LoadValue("selenium.baseUrl", (Uri)null, true);
      driverConfig.RelativeMicroceliumLogoPath = LoadValue("selenium.relativeMicroceliumLogoPath", "img/inteligenz.png");
      driverConfig.Username = LoadValue("selenium.username", (string)null);
      driverConfig.Password = LoadValue("selenium.password", (string)null);
      driverConfig.RelativeLoginUrl = LoadValue("selenium.relativeLoginUrl", "/");
      return driverConfig;
    }

    private ChromeOptions CreateChromeOptions(IWebDriverConfig browserConfig)
    {
      var opts = new ChromeOptions();
      opts.AddArguments("--disable-extensions");
      opts.AddArguments("--no-sandbox");
      opts.AddArguments($"--window-size={browserConfig.BrowserSize.Width},{browserConfig.BrowserSize.Height}");

      if (browserConfig.RunHeadless && downloadDirectory == null)
      {
        opts.AddArguments("--headless");
        opts.AddArguments("--disable-gpu");
        opts.AddArguments("--hide-scrollbars");
      }

      if (downloadDirectory != null)
        opts.AddUserProfilePreference("download.default_directory", downloadDirectory.FullName);

      options?.Invoke(opts);
      return opts;
    }
  }
}