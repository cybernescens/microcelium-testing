using System;
using System.Net.Http;

namespace Microcelium.Testing.Selenium;

/// <summary>
///   Selenium Driver Configuration Options
/// </summary>
public class WebDriverConfig
{
  public static readonly string SectionName = "WebDriver";

  private static readonly string BlankPage =
    "<html><body><div>...Loading Tests...</div></body></html>";

  private string baseUri = "https://localhost:44314";

  /// <summary>
  ///   This site Base URI
  /// </summary>
  public string BaseUri
  {
    get => baseUri;
    set =>
      baseUri = value.EndsWith("/", StringComparison.InvariantCultureIgnoreCase)
        ? value.Substring(0, value.Length - 1)
        : value;
  }

  /// <summary>
  /// The <see cref="Uri"/> form of <see cref="BaseUri"/>
  /// </summary>
  /// <returns></returns>
  public Uri GetBaseUri() => new(BaseUri);

  /// <summary>
  ///   Relative Redirect Path for Post-Login redirect
  /// </summary>
  public string RelativeLoginPath { get; set; } = "/";

  /// <summary>
  ///   <see cref="HttpContent"/> to be used as a response while attempting to authenticate
  /// </summary>
  public HttpContent WaitingContent { get; set; } = new StringContent(BlankPage);

  /// <summary>
  /// Browser Config Options
  /// </summary>
  public BrowserConfig Browser { get; set; } = new();
  
  /// <summary>
  /// Timeout Config Options
  /// </summary>
  public TimeoutConfig Timeout { get; set; } = new();

  /// <summary>
  /// Authentication Config Options
  /// </summary>
  public AuthenticationConfig Authentication { get; set; } = new();
}

/// <summary>
/// Browser Configuration Options
/// </summary>
public class BrowserConfig
{
  public static readonly string SectionName = nameof(WebDriverConfig.Browser);

  /// <summary>
  ///   The Assembly Qualified Name of the Selenium Driver
  /// </summary>
  public string DriverFactory { get; set; } = "Microcelium.Testing.Selenium.Chrome.ChromeDriverFactory";

  /// <summary>
  /// Browser Specific Configuration object
  /// </summary>
  public object? Properties { get;set; }

  /// <summary>
  ///   The dimensions of the browser. Defaults to <code>1280</code>x<code>1024</code>;
  /// </summary>
  public Size Size { get; set; } = new(1280, 1024);

  /// <summary>
  ///   Run the tests Headless? Default is <code>true</code>.
  /// </summary>
  public bool Headless { get; set; } = true;
}

/// <summary>
/// Time Configuration Options
/// </summary>
public class TimeoutConfig
{
  public static readonly string SectionName = nameof(WebDriverConfig.Timeout);

  /// <summary>
  ///   Page Load timeout. Default is <code>60</code>s
  /// </summary>
  public TimeSpan PageLoad { get; set; } = TimeSpan.FromSeconds(60);

  /// <summary>
  ///   Implicit wait timeout. Default is <code>30</code>s
  /// </summary>
  public TimeSpan Implicit { get; set; } = TimeSpan.FromSeconds(30);

  /// <summary>
  ///   The Browser timeout. Default is <code>120</code>s
  /// </summary>
  public TimeSpan Browser { get; set; } = TimeSpan.FromSeconds(120);

  /// <summary>
  ///   Implicit timeout for asynchronous java script
  /// </summary>
  public TimeSpan Script { get; set; } = TimeSpan.FromMinutes(5);

  /// <summary>
  ///   Implicit timeout for downloads
  /// </summary>
  public TimeSpan Download { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Authentication Configuration Options
/// </summary>
public class AuthenticationConfig
{
  public static readonly string SectionName = nameof(WebDriverConfig.Authentication);

  public readonly struct CredentialModes
  {
    public static readonly string None = "None";
    public static readonly string Local = "Local";
    public static readonly string KeyVault = "KeyVault";
  }

  /// <summary>
  /// The CredentialMode, use <code>Local</code> to use <see cref="Username"/> and <see cref="Password"/>
  /// and use <code>KeyVault</code> to use <see cref="KeyVaultUri"/>. In the Key Value we will look
  /// for a secret named WebDriver__Auth__{KeyVaultSecretName}_Username and WebDriver__Auth__{KeyVaultSecretName}_Password
  /// where the value of <code>KeyVaultSecretName</code> can be provided by <see cref="KeyVaultSecretName"/>.
  /// <see cref="KeyVaultSecretName"/> defaults to <code>Selenium</code>
  /// </summary>
  public string CredentialMode { get; set; } = "Local";

  /// <summary>
  /// This Key Vault is blocked by firewall and managed by RBAC, so
  /// <list type="number">
  /// <item>Be sure to add the client IP to the firewall</item>
  /// <item>If running locally as a developer ensure your AD Account can List/Get Secrets</item>
  /// <item>If running as an automation account that user can List/Get Secrets</item>
  /// </list>
  /// </summary>
  public string? KeyVaultUri { get; set; } = "https://admin-keyvault-shared.vault.azure.net/";

  /// <summary>
  /// The Secret that contains the Credential Password
  /// </summary>
  public string KeyVaultSecretName { get; set; } = "Admin-Credentials-SeleniumTest";

  /// <summary>
  /// The Username to login as
  /// </summary>
  public string Username { get; set; } = "SeleniumUser";

  /// <summary>
  /// The password to use
  /// </summary>
  public string? Password { get; set; }

  /// <summary>
  /// Path to initialize the site to that is anonymous. Selenium cannot import cookies
  /// for the domain otherwise
  /// </summary>
  public string AnonymousPath { get; set; } = "/favicon.ico";

  /// <summary>
  /// No Credentials
  /// </summary>
  /// <returns></returns>
  public bool NoCredentials() => 
    CredentialMode.Equals(CredentialModes.None, StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Are we using Local Credentials
  /// </summary>
  /// <returns></returns>
  public bool IsLocalCredentials() => 
    CredentialMode.Equals(CredentialModes.Local, StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Are we using remote credentials stored in Azure Key Vault
  /// </summary>
  /// <returns></returns>
  public bool IsKeyVaultCredentials() =>
    CredentialMode.Equals(CredentialModes.KeyVault, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// A basic record representing width and height
/// </summary>
/// <param name="Width">the width (x-vector)</param>
/// <param name="Height">the height (y-vector)</param>
public record Size(int Width = 0, int Height = 0);