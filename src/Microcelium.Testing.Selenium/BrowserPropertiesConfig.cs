namespace Microcelium.Testing.Selenium;

/// <summary>
///   A holder for the ultimate persister configuration. When processing configuration
///   We look for one key in the <see cref="BrowserPropertiesConfig" /> and will fail otherwise.
///   The value of the 'key' component is used to make the name of the concrete configuration
///   object. So if the 'key' is &quot;LocalDisk&quot; then we attempt to bind the value
///   for that key to an object of type &quot;LocalDiskCookiePersisterConfig&quot;.
///   An exception will be thrown if no such type can be loaded.
/// </summary>
/// <example>
///   <code>
///    /* test.settings.json:*/
///    {
///      &quot;WebDriver&quot;: {
///        &quot;Browser&quot;: {
///          &quot;Properties&quot;: {
///            &quot;Chrome&quot;: {
///              &quot;UserProfileDirectory&quot;: &quot;%APPDATA%\\.microcelium-testing\\selenium-chrome-profile&quot;,
///            }
///          }
///        }
///      }
///    }
/// 
///    // will bind the value at CookiePersister.LocalDisk to a type
///    // named &quot;LocalDiskCookiePersisterConfig&quot;
///   </code>
/// </example>
public class BrowserPropertiesConfig //: ConfigurationSection
{
  public static readonly string SectionName = "Properties";
  //public CookiePersisterConfig(IConfigurationRoot root, string path) : base(root, path) { }
}