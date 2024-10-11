using System;

namespace Microcelium.Testing.Selenium.Authentication;

/// <summary>
/// Configuration for <see cref="LocalDiskCookiePersister"/>
/// </summary>
public class LocalDiskCookiePersisterConfig
{
  /// <summary>
  /// An instance of the default configuration
  /// </summary>
  public static LocalDiskCookiePersisterConfig Default = new();

  /// <summary>
  /// Fullname of the persister implementation
  /// </summary>
  public static Type Type => typeof(LocalDiskCookiePersister);

  /// <summary>
  /// The Directory Path or Root Directory to Cookies are persisted in
  /// </summary>
  public string DirectoryPath { get; set; } = "%APPDATA%\\.microcelium-testing\\cookies";

  /// <summary>
  /// Should the process cleanup any expired files
  /// </summary>
  public bool DeleteExpired { get; set; } = true;

  /// <summary>
  /// Initialization Timeout in milliseconds
  /// </summary>
  public int InitializationTimeout { get; set; } = 5000;
}