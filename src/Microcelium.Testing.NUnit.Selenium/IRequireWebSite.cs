using Microcelium.Testing.Selenium.Pages;

namespace Microcelium.Testing.NUnit.Selenium
{
  [RequiresWebBrowser]
  public interface IRequireWebSite<TPage> where TPage : Page<TPage>
  {
    TPage StartPage { get; set; }
  }
}