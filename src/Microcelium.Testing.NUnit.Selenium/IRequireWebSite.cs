using Microcelium.Testing.Selenium.Pages;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  /// Decorator for when a test requires access to a web site page
  /// </summary>
  /// <typeparam name="TPage">the ype of <see cref="Page{TPage}"/></typeparam>
  [RequiresWebBrowser]
  public interface IRequireWebSite<TPage> where TPage : Page<TPage>
  {
    /// <summary>
    /// The <typeparamref name="TPage" /> navigated to
    /// </summary>
    TPage StartPage { get; set; }
  }
}