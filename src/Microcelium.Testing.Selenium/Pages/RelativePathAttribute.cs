using System;

namespace Microcelium.Testing.Selenium.Pages;

[AttributeUsage(AttributeTargets.Class)]
public class RelativePathAttribute : Attribute
{
  public RelativePathAttribute(string path)
  {
    if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
      path = "/" + path;

    if (path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
      path = path.Substring(0, path.Length - 1);

    Path = path;
  }

  /// <summary>
  /// The relative path required to get to the a <see cref="WebPage"/>
  /// </summary>
  public string Path { get; }
}