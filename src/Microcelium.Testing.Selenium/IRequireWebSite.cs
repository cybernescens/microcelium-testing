using Microcelium.Testing.Selenium.Pages;

namespace Microcelium.Testing.Selenium;

public interface IRequireWebSite<TStartPage> : IRequireSeleniumHost where TStartPage : Page<TStartPage>
{
  Landing<TStartPage> Site { get; set; }
}