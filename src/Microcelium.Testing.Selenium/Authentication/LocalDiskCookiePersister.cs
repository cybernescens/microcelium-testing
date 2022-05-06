using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Selenium.Authentication;

/// <summary>
/// Persists cookies to the local disk
/// </summary>
public class LocalDiskCookiePersister : ICookiePersister
{
  private static readonly ReaderWriterLockSlim slim = new();
  private static readonly string EligibleCharacters = "abcdefghijklmnopqrstuvwxyz";
  private static readonly int NameLength = 20;

  private static bool initialized;

  private readonly LocalDiskCookiePersisterConfig config;
  private readonly ILogger<LocalDiskCookiePersister> log;
  private readonly JsonSerializerOptions options;

  private string RootDirectory => Environment.ExpandEnvironmentVariables(config.DirectoryPath);
  private string CurrentDirectory => DateTime.Today.ToString(config.CurrentDirectoryDateFormat);
  private string TargetDirectory => Path.Combine(RootDirectory, CurrentDirectory);
  private string InitializationFile => Path.Combine(TargetDirectory, ".initialized");
  
  public LocalDiskCookiePersister(LocalDiskCookiePersisterConfig config, ILoggerFactory lf)
  {
    this.config = config;
    this.log = lf.CreateLogger<LocalDiskCookiePersister>();
    this.options = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
  }

  private void EnsureDirectory()
  {
    var root = Environment.ExpandEnvironmentVariables(config.DirectoryPath);
    log.LogDebug("Using root `{Directory}` for cookie persistence", root);

    if (!Directory.Exists(root))
    {
      Directory.CreateDirectory(root);
      log.LogDebug("`{Directory}` directory created", root);
    }

    if (config.DeleteExpired)
    {
      foreach (var dir in Directory
                 .EnumerateDirectories(root)
                 .Where(x => !x.Equals(CurrentDirectory, StringComparison.CurrentCultureIgnoreCase)))
      {
        try
        {
          Directory.Delete(dir, true);
        }
        catch (Exception e)
        {
          log.LogWarning(e, "Unable to cleanup older directory");
        }
      }
    }

    if (!Directory.Exists(TargetDirectory))
    {
      Directory.CreateDirectory(TargetDirectory);
      log.LogDebug("`{Directory}` directory created", TargetDirectory);
    }
  }

  private static string NewFilename() =>
    Enumerable.Range(0, NameLength)
      .Select(_ => EligibleCharacters[new Random().Next(0, EligibleCharacters.Length - 1)])
      .Aggregate(string.Empty, (acc, nxt) => $"{acc}{nxt}");

  /// <inheritdoc />
  public async Task Persist(CookieContainer container, object? state = null)
  {
    EnsureDirectory();

    slim.EnterUpgradeableReadLock();

    try
    {
      if (initialized)
        return;
      
      try
      {
        slim.EnterWriteLock();

        foreach (var cookie in container.GetAllCookies())
        {
          var name = NewFilename();
          var path = Path.Combine(TargetDirectory, name);
          await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
          await JsonSerializer.SerializeAsync(stream, cookie, options);
          stream.Close();
        }
        
        await File.WriteAllTextAsync(InitializationFile, CurrentDirectory);
        initialized = true;
      }
      catch (Exception e)
      {
        initialized = true;
        log.LogError(e, "error encountered persisting cookies");
      }
      finally
      {
        if (slim.IsWriteLockHeld)
          slim.ExitWriteLock();
      }
    }
    finally
    {
      if (slim.IsUpgradeableReadLockHeld)
        slim.ExitUpgradeableReadLock();
    }
  }

  /// <inheritdoc />
  public async Task<CookieContainer> Retrieve(object? state = null)
  {
    if (!Initialized)
    {
      slim.EnterReadLock();

      try
      {
        var poll = Task.Run(
          async () => {
            while (!initialized) await Task.Delay(50);
          });

        var expire = Task.Delay(config.InitializationTimeout);

        if (poll != Task.WhenAny(poll, expire))
          throw new InvalidOperationException("Timeout reached while waiting for cookie persister initialization");
      }
      finally
      {
        if (slim.IsReadLockHeld)
          slim.ExitReadLock();
      }
    }

    var container = new CookieContainer();

    foreach (var path in Directory.EnumerateFiles(TargetDirectory).Where(x => !x.Equals(InitializationFile, StringComparison.CurrentCulture)))
    {
      await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
      var cookie = await JsonSerializer.DeserializeAsync<Cookie>(stream, options);
      container.Add(cookie!);
    }

    return container;
  }

  public Task Reset()
  {
    var root = Environment.ExpandEnvironmentVariables(config.DirectoryPath);
    if (Directory.Exists(root))
      Directory.Delete(root, true);

    return Task.CompletedTask;
  }

  public bool Initialized => File.Exists(InitializationFile);
}