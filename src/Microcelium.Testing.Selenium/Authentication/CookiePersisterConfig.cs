namespace Microcelium.Testing.Selenium.Authentication;

/// <summary>
///   A holder for the ultimate persister configuration. When processing configuration
///   We look for one key in the <see cref="CookiePersisterConfig" /> and will fail otherwise.
///   The value of the 'key' component is used to make the name of the concrete configuration
///   object. So if the 'key' is &quot;LocalDisk&quot; then we attempt to bind the value
///   for that key to an object of type &quot;LocalDiskCookiePersisterConfig&quot;.
///   An exception will be thrown if no such type can be loaded.
/// </summary>
/// <example>
///   <code>
///    /* test.settings.json:*/
///    {
///      &quot;CookiePersister&quot;: {
///        &quot;LocalDisk&quot;: {
///          &quot;DirectoryPath&quot;: &quot;%APPDATA%\\.microcelium-testing\\cookies&quot;,
///          &quot;DeleteExpired&quot;: true
///        }
///      }
///    }
///
///    // will bind the value at CookiePersister.LocalDisk to a type
///    // named &quot;LocalDiskCookiePersisterConfig&quot;
///   </code>
/// </example>
public class CookiePersisterConfig //: ConfigurationSection
{
  public static readonly string SectionName = "CookiePersister";
  //public CookiePersisterConfig(IConfigurationRoot root, string path) : base(root, path) { }
}