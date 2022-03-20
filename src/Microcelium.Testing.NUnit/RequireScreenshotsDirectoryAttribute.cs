using Microcelium.Testing.Selenium;

namespace Microcelium.Testing;

public class RequireScreenshotsDirectoryAttribute : EnsureDirectoryAttribute
{
  public RequireScreenshotsDirectoryAttribute() : base(typeof(IRequireScreenshots), false, "Screenshots") { }
}