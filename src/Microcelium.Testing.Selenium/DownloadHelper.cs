using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  internal class DownloadHelper
  {
    private readonly IWebDriver webDriver;
    private readonly ILogger log;

    public DownloadHelper(IWebDriver webDriver, ILogger log)
    {
      this.webDriver = webDriver;
      this.log = log;
    }

    public FileInfo WaitForFileDownload(string directory, string fileMask, TimeSpan timeout)
    {
      var loops = timeout.TotalSeconds;
      return webDriver.WaitUntil(
        x =>
          {
            var count = 0;
            while (true)
            {
              log.LogInformation(
                "Looking for file in folder '{0}' matching '{1}' attempt '{2}' of '{3}'",
                directory,
                fileMask,
                count,
                loops);

              var matchingFile = FindFile(new DirectoryInfo(directory), fileMask);

              if (matchingFile != null)
              {
                log.LogInformation("Found file '{0}' with file size '{1:#,#} bytes'", matchingFile.Name, matchingFile.Length);
                return matchingFile;
              }

              if (count >= loops)
              {
                throw new FileNotFoundException($"Could not find file with mask '{fileMask}' in directory '{directory}");
              }

              count++;
              Thread.Sleep(TimeSpan.FromSeconds(1));
            }
          },
        timeout);
    }

    private FileInfo FindFile(DirectoryInfo directory, string fileMask)
      => directory.GetFiles(fileMask).SingleOrDefault();
  }
}