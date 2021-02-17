using Microcelium.Testing.Selenium.Pages;
using NUnit.Framework;

namespace Microcelium.Testing.NUnit.Selenium
{
  /// <summary>
  ///   Decorator for when a test requires access to a web site page
  /// </summary>
  /// <typeparam name="TWebPage">the ype of <see cref="IWebPage" /></typeparam>
  /// <typeparam name="TWebSite"></typeparam>
  [RequiresWebBrowser]
  public interface IRequireWebPage<TWebSite, TWebPage>
    where TWebSite : IWebSite
    where TWebPage : IWebPage
  {
    /// <summary>
    /// 
    /// </summary>
    TWebSite Site { get; set; }

    /// <summary>
    /// </summary>
    TWebPage Page { get; set; }
  }
}