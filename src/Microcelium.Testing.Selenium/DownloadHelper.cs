using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Selenium;

internal class DownloadHelper
{
  private readonly ILogger log;
  private readonly TimeSpan timeout;

  public DownloadHelper(TimeSpan timeout, ILoggerFactory lf)
  {
    this.timeout = timeout;
    log = lf.CreateLogger<DownloadHelper>();
  }

  public FileInfo? WaitForFileDownload(DirectoryInfo directory, string fileMask)
  {
    var cts = new CancellationTokenSource(timeout);
    return FileDownloadTask(directory, fileMask, cts.Token).GetAwaiter().GetResult();
  }

  private async Task<FileInfo?> FileDownloadTask(
    DirectoryInfo directory, 
    string fileMask, 
    CancellationToken ct)
  {
    for (var attempt = 0; !ct.IsCancellationRequested; attempt++)
    {
      log.LogInformation(
        "Looking for file in folder '{directory}' matching '{fileMask}' attempt '{count}'",
        directory,
        fileMask,
        attempt);

      var matchingFile = FindFile(directory, fileMask);
      if (matchingFile != null)
      {
        log.LogInformation(
          "Found file '{name}' with file size '{length:#,#} bytes'",
          matchingFile.Name,
          matchingFile.Length);

        return matchingFile;
      }

      await Task.Delay(1000, ct);
    }

    return null;
  }

  private FileInfo? FindFile(DirectoryInfo directory, string fileMask) => 
    directory.GetFiles(fileMask).SingleOrDefault();
}