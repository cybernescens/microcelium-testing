using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;

namespace Microcelium.Testing.Selenium;

public class BrowserScreenshotCapturer
{
  private readonly ILogger? log;
  private readonly IWebDriverExtensions driver;

  public BrowserScreenshotCapturer(IWebDriverExtensions driver, ILoggerFactory lf)
  {
    this.driver = driver;
    this.log = lf.CreateLogger<BrowserScreenshotCapturer>();
  }

  public string? SaveScreenshotForEachTab(string filePath)
  {
    log?.LogInformation("Saving screen shoot to path:\n{0}", filePath);

    try
    {
      var mergeImages = MergeImages(GetImageForAllOpenTabs());
      mergeImages.Save(filePath);
      log?.LogInformation($"Screen shot saved to:\n{filePath}");
      return filePath;
    }
    catch (Exception e)
    {
      log?.LogError("Failed to capture screenshot:\n{0}", e);
    }

    return null;
  }
  
  private Image ScreenshotAsImage()
  {
    var screenshot = driver.GetScreenshot();
    var memStream = new MemoryStream(screenshot.AsByteArray);
    return Image.FromStream(memStream);
  }

  private Image? TakeFullScreenScreenshot()
  {
    if (!driver.DriverType.IsAssignableTo(typeof(ChromeDriver)))
      return ScreenshotAsImage();

    try
    {
      driver.ScrollTo(0, 0);

      var totalWidth = driver.ExecuteScript<int>("return document.documentElement.scrollWidth;");
      var totalHeight = driver.ExecuteScript<int>("return document.documentElement.scrollHeight;");

      var viewportWidth = driver.ExecuteScript<int>("return document.documentElement.clientWidth;");
      var viewportHeight = driver.ExecuteScript<int>("return document.documentElement.clientHeight;");

      var rectangles = SplitDocumentIntoRectangles(totalHeight, viewportHeight, totalWidth, viewportWidth);

      var stitchedImage = new Bitmap(totalWidth, totalHeight);

      foreach (var rectangle in rectangles)
      {
        driver.ScrollTo(rectangle.Left, rectangle.Top);

        var screenshotImage = ScreenshotAsImage();

        var sourceRectangle = new Rectangle(
          viewportWidth - rectangle.Width,
          viewportHeight - rectangle.Height,
          rectangle.Width,
          rectangle.Height);

        var g = Graphics.FromImage(stitchedImage);
        g.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
      }

      return stitchedImage;
    }
    catch (Exception e)
    {
      log?.LogError("Failed to take full page screen shot:\n{0}", e);
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

  private static Image MergeImages(IEnumerable<Image> images)
  {
    var enumerable = (images ?? new Image[] { }).ToArray();

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
    var g = Graphics.FromImage(bitmap);
    var localWidth = 0;
    foreach (var image in enumerable)
    {
      g.DrawImage(image, localWidth, 0);
      localWidth += image.Width;
    }

    return bitmap;
  }

  private IEnumerable<Image> GetImageForAllOpenTabs()
  {
    foreach (var id in driver.WindowHandles)
    {
      driver.SwitchTo().Window(id);
      yield return TakeFullScreenScreenshot() ?? ScreenshotAsImage();
    }
  }
}