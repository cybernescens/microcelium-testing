using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;

namespace Microcelium.Testing;

/// <summary>
///   Helpers for assembly scanning operations.
/// </summary>
public class AssemblyScanner
{
  private const string MicroceliumCoreAssemblyName = "Microcelium.Testing";

  private static readonly string[] FileSearchPatternsToUse = { "*.dll", "*.exe" };

  private static readonly string[] DefaultAssemblyExclusions = {
    // selenium
    "selenium",
    "chromedriver",
    "edgedriver",
    "firefoxdriver",
  };

  internal readonly List<string> AssembliesToSkip = new();
  internal readonly List<Type> TypesToSkip = new();
  private readonly Assembly? assemblyToScan;

  private readonly AssemblyValidator assemblyValidator = new();
  private readonly string baseDirectoryToScan;
  internal bool ScanNestedDirectories;

  /// <summary>
  ///   Creates a new scanner that will scan the base directory of the current <see cref="AppDomain" />.
  /// </summary>
  public AssemblyScanner()
    : this(AppContext.BaseDirectory) { }

  /// <summary>
  ///   Creates a scanner for the given directory.
  /// </summary>
  public AssemblyScanner(string baseDirectoryToScan) { this.baseDirectoryToScan = baseDirectoryToScan; }

  internal AssemblyScanner(Assembly assemblyToScan) { this.assemblyToScan = assemblyToScan; }

  /// <summary>
  ///   Determines if the scanner should throw exceptions or not.
  /// </summary>
  public bool ThrowExceptions { get; set; } = true;

  /// <summary>
  ///   Determines if the scanner should scan assemblies loaded in the <see cref="AppDomain.CurrentDomain" />.
  /// </summary>
  public bool ScanAppDomainAssemblies { get; set; } = true;

  /// <summary>
  ///   Determines if the scanner should scan assemblies from the file system.
  /// </summary>
  public bool ScanFileSystemAssemblies { get; set; } = true;

  internal string CoreAssemblyName { get; set; } = MicroceliumCoreAssemblyName;

  internal string AdditionalAssemblyScanningPath { get; set; }

  /// <summary>
  ///   Traverses the specified base directory including all sub-directories, generating a list of assemblies that should be
  ///   scanned for handlers, a list of skipped files, and a list of errors that occurred while scanning.
  /// </summary>
  public AssemblyScannerResults GetScannableAssemblies()
  {
    var results = new AssemblyScannerResults();
    var processed = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    if (assemblyToScan != null)
    {
      if (ScanAssembly(assemblyToScan, processed))
        AddTypesToResult(assemblyToScan, results);

      return results;
    }

    // Always scan Core assembly
    var coreAssembly = typeof(AssemblyScanner).Assembly;
    if (ScanAssembly(coreAssembly, processed))
      AddTypesToResult(coreAssembly, results);

    if (ScanAppDomainAssemblies)
    {
      var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

      foreach (var assembly in appDomainAssemblies)
      {
        if (ScanAssembly(assembly, processed))
          AddTypesToResult(assembly, results);
      }
    }

    if (ScanFileSystemAssemblies)
    {
      var assemblies = new List<Assembly>();

      ScanAssembliesInDirectory(baseDirectoryToScan, assemblies, results);

      if (!string.IsNullOrWhiteSpace(AdditionalAssemblyScanningPath))
        ScanAssembliesInDirectory(AdditionalAssemblyScanningPath, assemblies, results);

      var platformAssembliesString = (string)AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES");

      if (!string.IsNullOrEmpty(platformAssembliesString))
      {
        var platformAssemblies = platformAssembliesString.Split(Path.PathSeparator);

        foreach (var platformAssembly in platformAssemblies)
        {
          if (TryLoadScannableAssembly(platformAssembly, results, out var assembly))
            assemblies.Add(assembly);
        }
      }

      foreach (var assembly in assemblies)
      {
        if (ScanAssembly(assembly, processed))
          AddTypesToResult(assembly, results);
      }
    }

    results.RemoveDuplicates();
    return results;
  }

  private void ScanAssembliesInDirectory(
    string directoryToScan,
    List<Assembly> assemblies,
    AssemblyScannerResults results)
  {
    foreach (var assemblyFile in ScanDirectoryForAssemblyFiles(directoryToScan, ScanNestedDirectories))
    {
      if (TryLoadScannableAssembly(assemblyFile.FullName, results, out var assembly))
        assemblies.Add(assembly);
    }
  }

  [DebuggerNonUserCode]
  private bool TryLoadScannableAssembly(string assemblyPath, AssemblyScannerResults results, out Assembly assembly)
  {
    assembly = null;

    if (IsExcluded(Path.GetFileNameWithoutExtension(assemblyPath)))
    {
      var skippedFile = new SkippedFile(assemblyPath, "File was explicitly excluded from scanning.");
      results.SkippedFiles.Add(skippedFile);

      return false;
    }

    assemblyValidator.ValidateAssemblyFile(assemblyPath, out var shouldLoad, out var reason);

    if (!shouldLoad)
    {
      var skippedFile = new SkippedFile(assemblyPath, reason);
      results.SkippedFiles.Add(skippedFile);

      return false;
    }

    try
    {
      var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
      assembly = context.LoadFromAssemblyPath(assemblyPath);
      return true;
    }
    catch (Exception ex) when (ex is BadImageFormatException || ex is FileLoadException)
    {
      results.ErrorsThrownDuringScanning = true;

      if (ThrowExceptions)
      {
        var errorMessage = $"Could not load '{assemblyPath}'. Consider excluding that assembly from the scanning.";
        throw new Exception(errorMessage, ex);
      }

      var skippedFile = new SkippedFile(assemblyPath, ex.Message);
      results.SkippedFiles.Add(skippedFile);

      return false;
    }
  }

  private bool ScanAssembly(Assembly? assembly, Dictionary<string, bool> processed)
  {
    if (assembly == null)
      return false;

    if (processed.TryGetValue(assembly.FullName!, out var value))
      return value;

    processed[assembly.FullName!] = false;

    if (assembly.GetName().Name == CoreAssemblyName)
      return processed[assembly.FullName!] = true;

    if (ShouldScanDependencies(assembly))
    {
      foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
      {
        var referencedAssembly = GetReferencedAssembly(referencedAssemblyName);
        var referencesCore = ScanAssembly(referencedAssembly, processed);
        if (referencesCore)
        {
          processed[assembly.FullName!] = true;
          break;
        }
      }
    }

    return processed[assembly.FullName!];
  }

  private Assembly? GetReferencedAssembly(AssemblyName assemblyName)
  {
    Assembly? referencedAssembly = null;

    try
    {
      referencedAssembly = Assembly.Load(assemblyName);
    }
    catch (Exception ex) when
      (ex is FileNotFoundException || ex is BadImageFormatException || ex is FileLoadException) { }

    if (referencedAssembly == null)
      referencedAssembly = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);

    return referencedAssembly;
  }

  internal static string FormatReflectionTypeLoadException(string fileName, ReflectionTypeLoadException e)
  {
    var sb = new StringBuilder($"Could not enumerate all types for '{fileName}'.");

    if (!e.LoaderExceptions.Any())
    {
      sb.AppendLine($"Exception message: {e}");
      return sb.ToString();
    }

    var nsbAssemblyName = typeof(AssemblyScanner).Assembly.GetName();
    var nsbPublicKeyToken = BitConverter.ToString(nsbAssemblyName.GetPublicKeyToken()).Replace("-", string.Empty)
      .ToLowerInvariant();

    var displayBindingRedirects = false;
    var files = new List<string>();
    var sbFileLoadException = new StringBuilder();
    var sbGenericException = new StringBuilder();

    foreach (var ex in e.LoaderExceptions)
    {
      var loadException = ex as FileLoadException;

      if (loadException?.FileName != null)
      {
        var assemblyName = new AssemblyName(loadException.FileName);
        var assemblyPublicKeyToken = BitConverter.ToString(assemblyName.GetPublicKeyToken()).Replace("-", string.Empty)
          .ToLowerInvariant();

        if (nsbAssemblyName.Name == assemblyName.Name &&
            nsbAssemblyName.CultureInfo.ToString() == assemblyName.CultureInfo.ToString() &&
            nsbPublicKeyToken == assemblyPublicKeyToken)
        {
          displayBindingRedirects = true;
          continue;
        }

        if (!files.Contains(loadException.FileName))
        {
          files.Add(loadException.FileName);
          sbFileLoadException.AppendLine(loadException.FileName);
        }

        continue;
      }

      sbGenericException.AppendLine(ex.ToString());
    }

    if (sbGenericException.Length > 0)
    {
      sb.AppendLine("Exceptions:");
      sb.Append(sbGenericException);
    }

    if (sbFileLoadException.Length > 0)
    {
      sb.AppendLine();
      sb.AppendLine("It looks like you may be missing binding redirects in the config file for the following assemblies:");
      sb.Append(sbFileLoadException);
      sb.AppendLine("For more information see http://msdn.microsoft.com/en-us/library/7wd6ex19(v=vs.100).aspx");
    }

    if (displayBindingRedirects)
    {
      sb.AppendLine();
      sb.AppendLine("Try to add the following binding redirects to the config file:");

      const string bindingRedirects = @"<runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
        <dependentAssembly>
            <assemblyIdentity name=""NServiceBus.Core"" publicKeyToken=""9fc386479f8a226c"" culture=""neutral"" />
            <bindingRedirect oldVersion=""0.0.0.0-{0}"" newVersion=""{0}"" />
        </dependentAssembly>
    </assemblyBinding>
</runtime>";

      sb.AppendLine(string.Format(bindingRedirects, nsbAssemblyName.Version.ToString(4)));
    }

    return sb.ToString();
  }

  private static List<FileInfo> ScanDirectoryForAssemblyFiles(string directoryToScan, bool scanNestedDirectories)
  {
    var fileInfo = new List<FileInfo>();
    var baseDir = new DirectoryInfo(directoryToScan);
    var searchOption = scanNestedDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    foreach (var searchPattern in FileSearchPatternsToUse)
    {
      foreach (var info in baseDir.GetFiles(searchPattern, searchOption))
        fileInfo.Add(info);
    }

    return fileInfo;
  }

  private bool IsExcluded(string assemblyNameOrFileName)
  {
    var isExplicitlyExcluded = AssembliesToSkip.Any(excluded => IsMatch(excluded, assemblyNameOrFileName));
    if (isExplicitlyExcluded)
      return true;

    var isExcludedByDefault = DefaultAssemblyExclusions.Any(exclusion => IsMatch(exclusion, assemblyNameOrFileName));
    if (isExcludedByDefault)
      return true;

    return false;
  }

  private static bool IsMatch(string expression1, string expression2) =>
    DistillLowerAssemblyName(expression1) == DistillLowerAssemblyName(expression2);

  private bool IsAllowedType(Type? type) =>
    type != null &&
    !type.IsValueType &&
    !IsCompilerGenerated(type) &&
    !TypesToSkip.Contains(type);

  private static bool IsCompilerGenerated(Type type) =>
    type.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null;

  private static string DistillLowerAssemblyName(string assemblyOrFileName)
  {
    var lowerAssemblyName = assemblyOrFileName.ToLowerInvariant();
    if (lowerAssemblyName.EndsWith(".dll") || lowerAssemblyName.EndsWith(".exe"))
      lowerAssemblyName = lowerAssemblyName.Substring(0, lowerAssemblyName.Length - 4);

    return lowerAssemblyName;
  }

  private void AddTypesToResult(Assembly assembly, AssemblyScannerResults results)
  {
    try
    {
      //will throw if assembly cannot be loaded
      results.Types.AddRange(assembly.GetTypes().Where(IsAllowedType));
    }
    catch (ReflectionTypeLoadException e)
    {
      results.ErrorsThrownDuringScanning = true;

      var errorMessage = FormatReflectionTypeLoadException(assembly.FullName!, e);
      if (ThrowExceptions)
        throw new Exception(errorMessage);

      results.Types.AddRange(e.Types.Where(IsAllowedType)!);
    }

    results.Assemblies.Add(assembly);
  }

  private bool ShouldScanDependencies(Assembly assembly)
  {
    if (assembly.IsDynamic)
      return false;

    var assemblyName = assembly.GetName();

    if (assemblyName.Name == CoreAssemblyName)
      return false;

    if (AssemblyValidator.IsRuntimeAssembly(assemblyName.GetPublicKeyToken()!))
      return false;

    if (IsExcluded(assemblyName.Name!))
      return false;

    return true;
  }
}

/// <summary>
/// Holds <see cref="AssemblyScanner.GetScannableAssemblies" /> results.
/// Contains list of errors and list of scannable assemblies.
/// </summary>
public class AssemblyScannerResults
{
  /// <summary>
  /// Constructor to initialize AssemblyScannerResults.
  /// </summary>
  public AssemblyScannerResults()
  {
    Assemblies = new List<Assembly>();
    Types = new List<Type>();
    SkippedFiles = new List<SkippedFile>();
  }

  /// <summary>
  /// List of successfully found and loaded assemblies.
  /// </summary>
  public List<Assembly> Assemblies { get; private set; }

  /// <summary>
  /// List of files that were skipped while scanning because they were a) explicitly excluded
  /// </summary>
  public List<SkippedFile> SkippedFiles { get; }

  /// <summary>
  /// True if errors where encountered during assembly scanning.
  /// </summary>
  public bool ErrorsThrownDuringScanning { get; internal set; }

  /// <summary>
  /// List of types.
  /// </summary>
  public List<Type> Types { get; private set; }

  internal void RemoveDuplicates()
  {
    Assemblies = Assemblies.Distinct().ToList();
    Types = Types.Distinct().ToList();
  }
}

public record SkippedFile
{
  internal SkippedFile(string filePath, string message)
  {
    FilePath = filePath;
    SkipReason = message;
  }

  public string FilePath { get; }
  public string SkipReason { get; }
}

internal class AssemblyValidator
{
  public void ValidateAssemblyFile(string assemblyPath, out bool shouldLoad, out string reason)
  {
    using var stream = File.OpenRead(assemblyPath);
    using var file = new PEReader(stream);

    var hasMetadata = false;

    try
    {
      hasMetadata = file.HasMetadata;
    }
    catch (BadImageFormatException) { }

    if (!hasMetadata)
    {
      shouldLoad = false;
      reason = "File is not a .NET assembly.";
      return;
    }

    var reader = file.GetMetadataReader();
    var assemblyDefinition = reader.GetAssemblyDefinition();

    if (!assemblyDefinition.PublicKey.IsNil)
    {
      var publicKey = reader.GetBlobBytes(assemblyDefinition.PublicKey);
      var publicKeyToken = GetPublicKeyToken(publicKey);

      if (IsRuntimeAssembly(publicKeyToken))
      {
        shouldLoad = false;
        reason = "File is a .NET runtime assembly.";
        return;
      }
    }

    shouldLoad = true;
    reason = "File is a .NET assembly.";
  }

  private static byte[] GetPublicKeyToken(byte[] publicKey)
  {
    using var sha1 = SHA1.Create();
    var hash = sha1.ComputeHash(publicKey);
    var publicKeyToken = new byte[8];

    for (var i = 0; i < 8; i++)
      publicKeyToken[i] = hash[^(i + 1)];

    return publicKeyToken;
  }

  public static bool IsRuntimeAssembly(byte[] publicKeyToken)
  {
    var tokenString = BitConverter.ToString(publicKeyToken).Replace("-", string.Empty).ToLowerInvariant();

    switch (tokenString)
    {
      case "b77a5c561934e089": // Microsoft tokens
      case "7cec85d7bea7798e":
      case "b03f5f7f11d50a3a":
      case "31bf3856ad364e35":
      case "cc7b13ffcd2ddd51":
      case "adb9793829ddae60":
      case "7e34167dcc6d6d8c": // Microsoft.Azure.ServiceBus
      case "23ec7fc2d6eaa4a5": // Microsoft.Data.SqlClient
        return true;
      default:
        return false;
    }
  }
}