using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium.Chrome;

public class ChromeDriverFactory
{
  public static IWebDriver Driver(WebDriverConfig configuration, RuntimeConfig runtime)
  {
    var path = File.Exists(Path.Combine(AppContext.BaseDirectory, "chromedriver.exe")) ? AppContext.BaseDirectory : ".";
    var service = ChromeDriverService.CreateDefaultService(path);
    service.HideCommandPromptWindow = true;

    var options = new ChromeOptions();
    options.AddArguments("--incognito");
    options.AddArguments("--disable-extensions");
    options.AddArguments("--no-sandbox");
    options.AddArguments($"--window-size={configuration.Browser.Size.Width},{configuration.Browser.Size.Height}");

    if (configuration.Browser.Headless && string.IsNullOrEmpty(runtime.DownloadDirectory))
    {
      options.AddArguments("--headless");
      options.AddArguments("--disable-gpu");
      options.AddArguments("--hide-scrollbars");
    }

    if (!string.IsNullOrEmpty(runtime.DownloadDirectory))
      options.AddUserProfilePreference("download.default_directory", runtime.DownloadDirectory);

    options.AddUserProfilePreference("download.prompt_for_download", false);
    options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

    return new ChromeDriver(service, options, configuration.Timeout.Browser);
  }
}