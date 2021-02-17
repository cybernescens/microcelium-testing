using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium
{
  internal class BrowserScreenshotCapturer
  {
    private readonly IWebDriver webDriver;
    private readonly ILogger log;

    public BrowserScreenshotCapturer(IWebDriver webDriver, ILogger log)
    {
      this.webDriver = webDriver;
      this.log = log;
    }

    public string SaveScreenshotForEachTab(string filePath)
    {
      log.LogDebug("Saving screen shot...");

      try
      {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        var directory = !string.IsNullOrEmpty(Path.GetExtension(filePath))
          ? new FileInfo(fullPath).Directory
          : new DirectoryInfo(fullPath);

        if (directory.Exists)
          directory.Delete(true);

        directory.Create();
        var mergeImages = MergeImages(GetImageForAllOpenTabs(webDriver));
        mergeImages.Save(fullPath);
        log.LogInformation($"Screen shot saved to:\n{fullPath}");
        return fullPath;
      }
      catch (Exception e)
      {
        log.LogWarning(e.InnerException, "Failed to capture screenshot");
      }

      return null;
    }

    private Image TakeFullScreenScreenshot(IWebDriver webDriver)
    {
      if (!(webDriver is ChromeDriver))
      {
        return ScreenshotAsImage(webDriver);
      }

      try
      {
        webDriver.ScrollTo(0, 0);

        var totalWidth = webDriver.ExecuteScript<int>("return document.documentElement.scrollWidth;");
        var totalHeight = webDriver.ExecuteScript<int>("return document.documentElement.scrollHeight;");

        var viewportWidth = webDriver.ExecuteScript<int>("return document.documentElement.clientWidth;");
        var viewportHeight = webDriver.ExecuteScript<int>("return document.documentElement.clientHeight;");

        var rectangles = SplitDocumentIntoRectangles(totalHeight, viewportHeight, totalWidth, viewportWidth);

        var stitchedImage = new Bitmap(totalWidth, totalHeight);
        foreach (var rectangle in rectangles)
        {
          webDriver.ScrollTo(rectangle.Left, rectangle.Top);
          Thread.Sleep(200);

          var screenshotImage = ScreenshotAsImage(webDriver);

          var sourceRectangle = new Rectangle(
            viewportWidth - rectangle.Width,
            viewportHeight - rectangle.Height,
            rectangle.Width,
            rectangle.Height);

          using (var g = Graphics.FromImage(stitchedImage))
            g.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        return stitchedImage;
      }
      catch (Exception e)
      {
        log.LogError("Failed to take full page screen shot:\n{0}", e);
      }

      return null;
    }

    private static IEnumerable<Rectangle> SplitDocumentIntoRectangles(
      int totalHeight,
      int viewportHeight,
      int totalWidth,
      int viewportWidth)
    {
      for (var i = 0; i < totalHeight; i += viewportHeight)
      {
        var newHeight = viewportHeight;
        // Fix if the Height of the Element is too big
        if (i + viewportHeight > totalHeight)
          newHeight = totalHeight - i;

        // Loop until the Total Width is reached
        for (var ii = 0; ii < totalWidth; ii += viewportWidth)
        {
          var newWidth = viewportWidth;
          // Fix if the Width of the Element is too big
          if (ii + viewportWidth > totalWidth)
            newWidth = totalWidth - ii;

          yield return new Rectangle(ii, i, newWidth, newHeight);
        }
      }
    }

    private static Image ScreenshotAsImage(IWebDriver webDriver)
    {
      var screenshot = ((ITakesScreenshot)webDriver).GetScreenshot();
      using (var memStream = new MemoryStream(screenshot.AsByteArray))
        return Image.FromStream(memStream);
    }

    private static Image MergeImages(IEnumerable<Image> images)
    {
      var enumerable = images.ToList();

      var width = 0;
      var height = 0;

      foreach (var image in enumerable)
      {
        width += image.Width;
        height = image.Height > height
          ? image.Height
          : height;
      }

      var bitmap = new Bitmap(width, height);
      using (var g = Graphics.FromImage(bitmap))
      {
        var localWidth = 0;
        foreach (var image in enumerable)
        {
          g.DrawImage(image, localWidth, 0);
          localWidth += image.Width;
        }
      }

      return bitmap;
    }

    private IEnumerable<Image> GetImageForAllOpenTabs(IWebDriver webDriver)
    {
      foreach (var id in webDriver?.WindowHandles ?? new ReadOnlyCollection<string>(new string[0]))
      {
        webDriver.SwitchTo().Window(id);
        yield return TakeFullScreenScreenshot(webDriver) ?? ScreenshotAsImage(webDriver);
      }
    }
  }
}