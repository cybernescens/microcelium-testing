using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;

namespace Microcelium.Testing.Selenium;

/// <summary>
/// Encapsulates a User's credentials
/// </summary>
public record UserCredentials(string Username, SecureString Password)
{
  /// <summary>
  /// The username
  /// </summary>
  public string Username { get; } = Username;

  /// <summary>
  /// The password
  /// </summary>
  public SecureString Password { get; } = Password;
}

/// <summary>
/// Retrieves <see cref="UserCredentials"/> for logging in via Selenium
/// </summary>
public interface ICredentialProvider
{
  /// <summary>
  /// Resolves user credentials from <see cref="AuthenticationConfig"/>
  /// </summary>
  /// <returns></returns>
  Task<UserCredentials> FromConfig();

  /// <summary>
  /// Converts a secure string to a regular one
  /// </summary>
  /// <param name="value">the secure string</param>
  /// <returns></returns>
  string SecureStringToString(SecureString value);
}

/// <summary>
/// Shared functionality for Credential Providers
/// </summary>
public abstract class CredentialProvider : ICredentialProvider
{
  protected AuthenticationConfig Config { get; }

  protected CredentialProvider(AuthenticationConfig config) { Config = config; }

  protected void Validate(string name, Func<AuthenticationConfig, string?> property)
  {
    if (string.IsNullOrEmpty(property(Config)))
      throw new InvalidOperationException(
        $"{nameof(AuthenticationConfig)}.{name} is required");
  }

  protected SecureString AsSecureString(Func<string> password)
  {
    var secure = new SecureString();
    foreach (var c in password())
      secure.AppendChar(c);

    return secure;
  }

  /// <inheritdoc />
  public abstract Task<UserCredentials> FromConfig();

  /// <inheritdoc />
  public string SecureStringToString(SecureString value)
  {
    var valuePtr = IntPtr.Zero;
    try
    {
      valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
      return Marshal.PtrToStringUni(valuePtr)!;
    }
    finally
    {
      Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
    }
  }
}

/// <summary>
/// Retrieves credentials from Azure Key Vault
/// </summary>
public class KeyVaultCredentialProvider : CredentialProvider
{
  private readonly AuthenticationConfig config;
  private readonly SecretClient client;
  private readonly ConcurrentDictionary<string, UserCredentials> cache = new();

  public KeyVaultCredentialProvider(AuthenticationConfig config, SecretClient client) : base(config)
  {
    this.config = config;
    this.client = client;
  }

  public override async Task<UserCredentials> FromConfig()
  {
    Validate(nameof(AuthenticationConfig.Username), x => x.Username);
    Validate(nameof(AuthenticationConfig.KeyVaultSecretName), x => x.KeyVaultSecretName);

    if (cache.TryGetValue(config.Username!, out var credentials))
      return credentials;

    var response = await client.GetSecretAsync(config.KeyVaultSecretName);
    var password = AsSecureString(() => response.Value.Value);
    credentials = new UserCredentials(config.Username!, password);

    return cache.GetOrAdd(config.Username!, credentials);
  }
}

/// <summary>
/// Retrieves credentials from local configuration
/// </summary>
public class LocalCredentialProvider : CredentialProvider
{
  private readonly AuthenticationConfig config;

  public LocalCredentialProvider(AuthenticationConfig config) : base(config) { this.config = config; }

  public override Task<UserCredentials> FromConfig()
  {
    Validate(nameof(AuthenticationConfig.Username), x => x.Username);
    Validate(nameof(AuthenticationConfig.Password), x => x.Password);

    var password = AsSecureString(() => Config.Password!);

    return Task.FromResult(new UserCredentials(config.Username!, password));
  }
}