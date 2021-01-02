using System.IO;

namespace Microcelium.Testing
{
  public interface IRequireDownloadDirectory
  {
    DirectoryInfo DownloadDirectory { get; set; }
  }
}