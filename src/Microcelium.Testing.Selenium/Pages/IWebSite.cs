using System;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  /// <summary>
  /// Conceptually represents a "Web Site" which hosts many "Web Pages"
  /// </summary>
  public interface IWebSite
  {
    /// <summary>
    /// Prepare page uses an implanted <see cref="PageFactory"/> to create <see cref="IWebPage"/>s
    /// </summary>
    /// <typeparam name="TPage"></typeparam>
    /// <returns></returns>
    TPage PreparePage<TPage>() where TPage : IWebPage;

    /// <summary>
    /// A factory method responsible for creating <see cref="IWebPage"/> in the proper scope.
    /// </summary>
    Func<Type, IWebPage> PageFactory { get; }

    /// <summary>
    /// The currently loaded page's title
    /// </summary>
    string CurrentTitle { get; }

    /// <summary>
    /// the Selenium <see cref="IWebDriver"/>
    /// </summary>
    IWebDriver Driver { get; }

    /// <summary>
    /// the Configured <see cref="WebDriverConfig"/> that in turn configured the <see cref="IWebDriver"/>
    /// </summary>
    WebDriverConfig Config { get; }
  }
  
  /// <inheritdoc />
  public abstract class WebSite : IWebSite
  {
    /// <summary>
    /// Instantiates an <see cref="WebSite"/>
    /// </summary>
    /// <param name="driver">the Selenium <see cref="IWebDriver"/></param>
    /// <param name="config">the configured <see cref="WebDriverConfig"/></param>
    protected WebSite(IWebDriver driver, IOptions<WebDriverConfig> config)
    {
      Driver = driver;
      Config = config.Value;
    }

    /// <inheritdoc />
    TPage IWebSite.PreparePage<TPage>() => (TPage) PageFactory(typeof(TPage));

    /// <inheritdoc />
    public Func<Type, IWebPage> PageFactory { get; internal set; }

    /// <inheritdoc />
    public string CurrentTitle => Driver.Title;

    /// <inheritdoc />
    public IWebDriver Driver { get; }

    /// <inheritdoc />
    public WebDriverConfig Config { get; }
  }

  /// <summary>
  /// Fired when a Page is Loading and Loaded
  /// </summary>
  public record ComponentLoadEvent
  {
    /// <summary>
    /// The entire <see cref="Uri"/> the Page represents
    /// </summary>
    public Uri Path;

    /// <summary>
    /// The <see cref="IWebPage"/> responsible for the event
    /// </summary>
    public IWebPage Page;
  }
}