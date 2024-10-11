﻿using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

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

  public static readonly string CredentialModeLocal = "Local";
  public static readonly string CredentialModeKeyVault = "KeyVault";

  /// <summary>
  /// The Client ID of the Proxy Application. Public Client Authorization Flow should be enabled as well
  /// </summary>
  public string? ClientId { get; set; }

  /// <summary>
  /// The CredentialMode, use <code>Local</code> to use <see cref="Username"/> and <see cref="Password"/>
  /// and use <code>KeyVault</code> to use <see cref="KeyVaultUri"/>. In the Key Value we will look
  /// for a secret named WebDriver__Auth__{KeyVaultSecretPrefix}_Username and WebDriver__Auth__{KeyVaultSecretPrefix}_Password
  /// where the value of <code>KeyVaultSecretPrefix</code> can be provided by <see cref="KeyVaultSecretPrefix"/>.
  /// <see cref="KeyVaultSecretPrefix"/> defaults to <code>Selenium</code>
  /// </summary>
  public string CredentialMode { get; set; } = "Local";
  
  /// <summary>
  /// The Username to login as
  /// </summary>
  public string? KeyVaultUri { get; set; }

  /// <summary>
  /// The Username to login as
  /// </summary>
  public string KeyVaultSecretPrefix { get; set; } = "Selenium";

  /// <summary>
  /// The Username to login as
  /// </summary>
  public string? Username { get; set; }

  /// <summary>
  /// The password to use
  /// </summary>
  public string? Password { get; set; }

  /// <summary>
  /// The scopes to request with OpenId
  /// </summary>
  public string[] Scopes { get; set; } = Array.Empty<string>();

  /// <summary>
  /// Are we using Local Credentials
  /// </summary>
  /// <returns></returns>
  public bool IsLocalCredentials() => 
    CredentialMode.Equals(CredentialModeLocal, StringComparison.OrdinalIgnoreCase);

  /// <summary>
  /// Are we using remote credentials stored in Azure Key Vault
  /// </summary>
  /// <returns></returns>
  public bool IsKeyVaultCredentials() =>
    CredentialMode.Equals(CredentialModeKeyVault, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// A basic record representing width and height
/// </summary>
/// <param name="Width">the width (x-vector)</param>
/// <param name="Height">the height (y-vector)</param>
public record Size(int Width = 0, int Height = 0);