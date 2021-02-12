using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  public class WebDriverConfig 
  {
    /// <summary>
    /// Configuration parameter:
    /// <c>webdriver.browser.type</c>
    /// </summary>
    public string BrowserType { get; set; } = typeof(ChromeDriver).FullName;

    private string BrowserSize { get; set; } = "1600,1200";
    public static readonly string SectionName = "selenium";

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.type</c>
    /// </summary>
    public (int Width, int Height) GetBrowserSize()
    {
      if (string.IsNullOrEmpty(BrowserSize))
        return (1600, 1200);

      var parts = BrowserSize.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
      if (parts.Length != 2)
        throw new InvalidOperationException("browserSize does not appear to be valid. Should be 'width,height'");

      return (parts[0], parts[1]);
    }

    /// <summary>
    ///   Configuration parameter: <c>webdriver.browser.runheadless</c>
    /// </summary>
    public bool RunHeadless { get; set; } = true;

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.pageload</c>
    /// </summary>
    public TimeSpan PageLoadTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.implicitwait</c>
    ///   This really should be zero
    /// </summary>
    public TimeSpan ImplicitTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    ///   Configuration parameter: <c>webdriver.timeout.browser</c>
    ///   Timeout waiting for browser to respond. Default to 60 seconds
    /// </summary>
    public TimeSpan BrowserTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The Base URL of the site the driver will be working with
    /// </summary>
    private string BaseUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Uri GetBaseUrl() => new Uri(BaseUrl);

    /// <summary>
    /// If authentication is required, the username
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// If authentication is required, the password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The secure password
    /// </summary>
    /// <returns></returns>
    public SecureString GetPassword()
    {
      var pw = new SecureString();
      foreach(var c in Password)
        pw.AppendChar(c);

      return pw;
    }

    /// <summary>
    /// Relative Redirect URL after logging in
    /// </summary>
    public string RelativeLoginUrl { get; set; } = "/";

    /// <summary>
    /// Relative path to an inteligenz logo, should be a path
    /// that requires no authentication
    /// </summary>
    public string RelativeLogoPath { get; set; } = "/favicon.ico";

    /// <summary>
    /// 
    /// </summary>
    public string AzureClientId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AzureClientSecret { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AzureTenantId { get; set; } = "ccb9ffa9-f1ed-4f51-9b6a-e5cf6d8275c7";

    /// <summary>
    /// 
    /// </summary>
    private string AzureClientAuthority { get; set; } = "https://login.microsoftonline.com/<TenantId>/";

    /// <summary>
    /// CSS Selector used to validate login was successful
    /// </summary>
    public string LoggedInValidationSelector { get; set; } = "button.user-name";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Uri GetAzureClientAuthority() => new Uri(AzureClientAuthority.Replace("<TenantId>", AzureTenantId));

    /// <summary>
    ///   Gets the configured <see cref="ChromeOptions" />
    /// </summary>
    public ChromeOptions GetChromeOptions(Action<ChromeOptions> config = null, string downloadDirectory = null)
    {
      var size = GetBrowserSize();
      var opts = new ChromeOptions();
      opts.AddArgument("--incognito");
      opts.AddArguments("--disable-extensions");
      opts.AddArguments("--no-sandbox");
      opts.AddArguments($"--window-size={size.Width},{size.Height}");

      if (RunHeadless && string.IsNullOrEmpty(downloadDirectory))
      {
        opts.AddArguments("--headless");
        opts.AddArguments("--disable-gpu");
        opts.AddArguments("--hide-scrollbars");
      }

      opts.AddLocalStatePreference("download.prompt_for_download", false);
      opts.AddLocalStatePreference("plugins.always_open_pdf_externally", true);
      opts.AddLocalStatePreference("browser.enabled_labs_experiments", new[]
        { "same-site-by-default-cookies@2", "cookies-without-same-site-must-be-secure@2" });

      if (downloadDirectory != null)
        opts.AddLocalStatePreference("download.default_directory", downloadDirectory);

      config?.Invoke(opts);
      return opts;
    }
  }

  public static class WebDriverConfigExtensions
  {
    public static IServiceCollection AddWebDriverConfig(this IServiceCollection services, Action<WebDriverConfig> config = null)
    {
      var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, false)
        .AddEnvironmentVariables("selenium_")
        .Build();

      WebDriverConfig wdc = new WebDriverConfig();
      configuration.Bind(wdc, opt => { opt.BindNonPublicProperties = true; });
      config?.Invoke(wdc);
      services.TryAddSingleton(Options.Create(wdc));
      return services;
    }

    public static IServiceCollection AddInMemoryWebDriverConfig(this IServiceCollection services, IEnumerable<KeyValuePair<string, string>> args)
    {
      var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(args)
        .Build();

      WebDriverConfig wdc = new WebDriverConfig();
      configuration.Bind(wdc, opt => { opt.BindNonPublicProperties = true; });
      services.TryAddSingleton(Options.Create(wdc));
      return services;
    }
  }
}