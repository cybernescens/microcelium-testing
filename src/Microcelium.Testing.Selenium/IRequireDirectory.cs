using System.IO;

namespace Microcelium.Testing.Selenium;

public interface IRequireDirectory : IRequireHost { }

public class DirectoryProvider
{
  private readonly string directory;
  public DirectoryProvider(string directory) { this.directory = directory; }

  public string GetDirectory()
  {
    Directory.CreateDirectory(directory);
    return directory;
  }
}

public class DownloadDirectoryProvider : DirectoryProvider
{
  public DownloadDirectoryProvider(string directory) : base(directory) { }
}

public class ScreenshotDirectoryProvider : DirectoryProvider
{
  public ScreenshotDirectoryProvider(string directory) : base(directory) { }
}

public class ContentRootDirectoryProvider : DirectoryProvider
{
  public ContentRootDirectoryProvider(string directory) : base(directory) { }
}