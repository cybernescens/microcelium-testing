using Microcelium.Testing.Selenium;

namespace Microcelium.Testing;

public class RequireDownloadDirectoryAttribute : EnsureDirectoryAttribute
{
  public RequireDownloadDirectoryAttribute() : base(typeof(IRequireDownloadDirectory), true, "Download") { }
}