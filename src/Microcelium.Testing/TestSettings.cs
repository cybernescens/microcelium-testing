namespace Microcelium.Testing;

/// <summary>
/// Some test settings to override default behavior
/// </summary>
public class TestSettings
{
  public SeleniumSettings Selenium { get; set; } = new();
}

/// <summary>
/// Selenium related settings
/// </summary>
public class SeleniumSettings
{
  /// <summary>
  /// Options pertaining to screenshots for selenium
  /// </summary>
  public ScreenshotOptions Screenshots { get; set; } = ScreenshotOptions.Default;
}

/// <summary>
/// Options related to saving screenshots
/// </summary>

public enum ScreenshotOptions
{
  /// <summary>
  /// Default captures at test start and end
  /// </summary>
  Default,

  /// <summary>
  /// Captures at test start and on test failure only
  /// </summary>
  Failures,

  /// <summary>
  /// Captures at test end only and only failures
  /// </summary>
  FailuresAtEnd,

  /// <summary>
  /// Does not capture
  /// </summary>
  Suppress
}