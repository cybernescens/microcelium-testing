using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium.Chrome;

public class ChromeDriverFactory
{
  public static IWebDriver Driver(WebDriverConfig configuration, WebDriverRuntime runtime)
  {
    var path = File.Exists(Path.Combine(AppContext.BaseDirectory, "chromedriver.exe")) ? AppContext.BaseDirectory : ".";
    var service = ChromeDriverService.CreateDefaultService(path);
    service.HideCommandPromptWindow = true;
    
    var options = new ChromeOptions();
    options.PageLoadStrategy = PageLoadStrategy.Normal;

    if (runtime.AuthenticationRequired)
    {
      Directory.CreateDirectory(Path.Combine(UserDataDirectory, ProfileName));
      options.AddArgument($"user-data-dir={UserDataDirectory.Replace("\\", "/")}");
      options.AddArgument($"profile-directory=Selenium");
      options.AddArgument("allow-profiles-outside-user-dir");
    }
    else
    {
      options.AddArgument("incognito");
    }

    //no-initial-navigation ⊗
    //options.AddArgument("no-default-browser-check");
    //options.AddArgument("no-initial-navigation");
    //options.AddArgument("no-first-run");
    //options.AddArgument("ignore-user-profile-mapping-for-tests");
    //options.AddArgument("disable-notifications");
    //options.AddArgument("dom-automation");
    //options.AddArgument("browser-test");
    options.AddArgument("disable-extensions");
    options.AddArgument("disable-features=ChromeWhatsNewUI");
    options.AddArgument($"window-size={configuration.Browser.Size.Width},{configuration.Browser.Size.Height}");

    if (configuration.Browser.Headless && string.IsNullOrEmpty(runtime.DownloadDirectory))
    {
      options.AddArgument("headless");
      options.AddArgument("disable-gpu");
      options.AddArgument("hide-scrollbars");
    }

    if (!string.IsNullOrEmpty(runtime.DownloadDirectory))
      options.AddUserProfilePreference("download.default_directory", runtime.DownloadDirectory);

    options.AddUserProfilePreference("download.prompt_for_download", false);
    options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

    return new ChromeDriver(service, options, configuration.Timeout.Browser);
  }

  public static string UserDataDirectory =>
    Path.Combine(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), "Google", "Chrome", "Selenium Data");

  public static string ProfileName => "Selenium";
}