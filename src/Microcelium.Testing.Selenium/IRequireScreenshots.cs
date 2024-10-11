namespace Microcelium.Testing.Selenium;

public interface IRequireScreenshots : IRequireDirectory
{
  string? ScreenshotDirectory { get; set; }
}