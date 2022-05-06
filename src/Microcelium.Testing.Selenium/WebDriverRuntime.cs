namespace Microcelium.Testing.Selenium;

public class WebDriverRuntime
{
  public bool AuthenticationRequired { get; set; }
  public string? DownloadDirectory { get; set; }
  public string? ScreenshotDirectory { get; set; } 
  public string? ContentRootDirectory { get; set; }
}