using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Microcelium.Testing.Selenium;

public interface IRequireDirectory : IRequireHost { }

public interface IDirectoryProviderFactory
{
  DirectoryProvider Create(string key);
  DirectoryProvider Create<T>() where T : IRequireDirectory;
}

internal class DirectoryProviderFactory : IDirectoryProviderFactory
{
  private readonly IDictionary<string, Func<DirectoryProvider>> keyedFactory;
  private readonly IDictionary<Type, Func<DirectoryProvider>> typedFactory;

  public DirectoryProviderFactory(DirectoryProvider[] providers)
  {
    keyedFactory = new ReadOnlyDictionary<string, Func<DirectoryProvider>>(
      providers.ToDictionary(x => x.Key, x => (Func<DirectoryProvider>)(() => x)));

    typedFactory = new ReadOnlyDictionary<Type, Func<DirectoryProvider>>(
      providers.ToDictionary(x => x.Type, x => (Func<DirectoryProvider>)(() => x)));
  }

  public DirectoryProvider Create(string key)
  {
    if (!keyedFactory.TryGetValue(key, out var provider))
      throw new InvalidOperationException($"No {nameof(DirectoryProvider)} for key `{key}`");

    return provider();
  }

  public DirectoryProvider Create<T>() where T : IRequireDirectory
  {
    if (!typedFactory.TryGetValue(typeof(T), out var provider))
      throw new InvalidOperationException($"No {nameof(DirectoryProvider)} for key `{typeof(T).FullName}`");

    return provider();
  }
}

public abstract class DirectoryProvider
{
  private readonly string? name;
  private readonly bool purge;

  public DirectoryProvider(string? name, bool purge)
  {
    this.name = name;
    this.purge = purge;
  }

  public abstract string Key { get; }
  public abstract Type Type { get; }

  public string GetDirectory(string root)
  {
    var path = string.IsNullOrEmpty(name) ? root : Path.Combine(root, name);
    if (Directory.Exists(path) && purge)
      Directory.Delete(path, true);

    Directory.CreateDirectory(path);
    return path;
  }
}

public class DownloadDirectoryProvider : DirectoryProvider
{
  public DownloadDirectoryProvider() : base("Download", true) { }
  public override string Key => "download";
  public override Type Type => typeof(IRequireDownloadDirectory);
}

public class ScreenshotDirectoryProvider : DirectoryProvider
{
  public ScreenshotDirectoryProvider() : base("Screenshot", false) { }
  public override string Key => "screenshot";
  public override Type Type => typeof(IRequireScreenshots);
}

public class ContentRootDirectoryProvider : DirectoryProvider
{
  public ContentRootDirectoryProvider() : base(null, false) { }
  public override string Key => "content-root";
  public override Type Type => typeof(IRequireContentRootDirectory);
}