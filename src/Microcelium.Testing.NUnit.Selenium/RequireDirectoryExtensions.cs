using System;
using Microcelium.Testing.Selenium;
using NUnit.Framework.Internal;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Helpers for getting directories
  /// </summary>
  public static class RequireDirectoryExtensions
  {
    internal const string DownloadDirectoryPropertyKey = nameof(DownloadDirectoryPropertyKey);
    internal const string ScreenshotDirectoryPropertyKey = nameof(ScreenshotDirectoryPropertyKey);

    /// <summary>
    /// Gets the current download directory
    /// </summary>
    /// <param name="irdd"></param>
    /// <returns></returns>
    public static string GetDownloadDirectory(this IRequireDownloadDirectory irdd) =>
      GetContextDirectory(DownloadDirectoryPropertyKey);

    /// <summary>
    /// Gets the current screenshot directory
    /// </summary>
    /// <param name="irs"></param>
    /// <returns></returns>
    public static string GetScreenshotDirectory(this IRequireScreenshots irs) =>
      GetContextDirectory(ScreenshotDirectoryPropertyKey);

    private static string GetContextDirectory(string contextKey) =>
      Convert.ToString(TestExecutionContext.CurrentContext.GetSuiteProperty(contextKey));
  }
}