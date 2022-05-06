namespace Microcelium.Testing.Selenium;

public interface IConfigureWebDriverConfig : IRequireSeleniumHost
{
  void Configure(WebDriverConfig config);
}