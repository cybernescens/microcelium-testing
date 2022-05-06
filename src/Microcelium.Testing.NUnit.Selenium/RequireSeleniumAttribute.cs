using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microcelium.Testing.Selenium.Authentication;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Selenium;

public class RequireSeleniumAttribute : RequireHostAttribute
{
  private static readonly Type RequiresWebType = typeof(IRequireWebSite<>);
  
  private string? downloadDirectory;
  private bool screenshotsRequired;
  private string? screenshotsDirectory;
  private bool websiteRequired;

  private IRequireSeleniumHost fixture = null;
  private IWebDriverExtensions driver = null!;
  private WebDriverConfig config = new();

  private Type? websiteType;
  private WebSite? website;
  private ServiceDescriptor[] webPageDescriptors = Array.Empty<ServiceDescriptor>();
  private bool downloadRequired;
  private MethodInfo websiteSetProperty;
  private ScreenshotOptions screenshotOptions = new();
  private MethodInfo websiteInitialize;
  private AssemblyScannerResults scannedAssemblies;
  private ICookiePersister cookiePersister;
  private IServiceScope seleniumScope = null!;

  protected override IRequireHost Fixture => fixture;

  protected override void EnsureFixture(ITest test)
  {
    fixture = EnsureFixture<RequireSeleniumAttribute, IRequireSeleniumHost>(test);
  }

  protected override IHostBuilder CreateHostBuilder() => new HostBuilder();

  protected override IHost CreateHost(IHostBuilder builder) => builder.Build();

  protected override void DefaultServicesConfiguration(HostBuilderContext ctx, IServiceCollection services)
  {
    ctx.Configuration.Bind(WebDriverConfig.SectionName, config);
    ctx.Configuration.Bind($"{WebDriverConfig.SectionName}__{BrowserConfig.SectionName}", config.Browser);
    ctx.Configuration.Bind($"{WebDriverConfig.SectionName}__{TimeoutConfig.SectionName}", config.Timeout);
    ctx.Configuration.Bind($"{WebDriverConfig.SectionName}__{AuthenticationConfig.SectionName}", config.Authentication);

    services.AddSingleton(config.Browser);
    services.AddSingleton(config.Timeout);
    services.AddSingleton(config.Authentication);
    services.AddSingleton(config);

    services.AddSingleton<IDirectoryProviderFactory>(
      sp => new DirectoryProviderFactory(sp.GetServices<DirectoryProvider>().ToArray()));

    services.AddSingleton<DirectoryProvider, DownloadDirectoryProvider>();
    services.AddSingleton<DirectoryProvider, ScreenshotDirectoryProvider>();
    services.AddSingleton<DirectoryProvider, ContentRootDirectoryProvider>();

    services.AddSingleton<WebDriverFactory>();

    services.AddScoped(sp => {
      var test = sp.GetRequiredService<ITest>();

      var runtime = new WebDriverRuntime {
        AuthenticationRequired = test.Fixture is IRequireAuthentication,
        ContentRootDirectory = ctx.HostingEnvironment.ContentRootPath
      };

      if (test.Fixture is IRequireDownloadDirectory)
      {
        var provider = sp.GetRequiredService<IDirectoryProviderFactory>().Create<IRequireDownloadDirectory>();
        runtime.DownloadDirectory = provider.GetDirectory(ctx.HostingEnvironment.ContentRootPath);
      }

      if (test.Fixture is IRequireScreenshots)
      {
        var provider = sp.GetRequiredService<IDirectoryProviderFactory>().Create<IRequireScreenshots>();
        runtime.ScreenshotDirectory = provider.GetDirectory(ctx.HostingEnvironment.ContentRootPath);
      }
      
      return runtime;
    });

    RegisterCookiePersister(ctx, services);

    if (config.Authentication.NoCredentials())
    {
      services.AddSingleton<ICredentialProvider, NoCredentialProvider>();
    }
    else if (config.Authentication.IsLocalCredentials())
    {
      services.AddSingleton<ICredentialProvider, LocalCredentialProvider>();
    }
    else if (config.Authentication.IsKeyVaultCredentials())
    {
      services.AddSingleton(sp => {
        var cfg = sp.GetRequiredService<AuthenticationConfig>();
        return new SecretClient(new Uri(cfg.KeyVaultUri!), new DefaultAzureCredential());
      });

      services.AddSingleton<ICredentialProvider, KeyVaultCredentialProvider>();
    }
    
    services.AddScoped(
      sp => {
        var runtime = sp.GetRequiredService<WebDriverRuntime>();
        return sp.GetRequiredService<WebDriverFactory>().Create(runtime);
      });

    services.AddScoped<IWebDriverExtensions, WebDriverAdapter>();

    if (websiteRequired && websiteType != null)
    {
      services.AddScoped(websiteType, websiteType);
      services.AddScoped(typeof(WebSite), websiteType);

      /* use scanner to register all possible combinations */
      foreach (var descriptor in webPageDescriptors)
        services.Add(descriptor);
    }
  }

  private void RegisterCookiePersister(HostBuilderContext ctx, IServiceCollection services)
  {
    var sub = ctx.Configuration.GetSection(CookiePersisterConfig.SectionName);
    var kvp = sub.AsEnumerable(true).FirstOrDefault();

    if (string.IsNullOrEmpty(kvp.Key))
    {
      services.AddSingleton<ICookiePersister, NoOpCookiePersister>();
      return;
    }

    var configurationTypeName = $"{kvp.Key}{nameof(CookiePersisterConfig)}";
    var configurationType = scannedAssemblies
      .Types
      .SingleOrDefault(x => x.Name.Equals(configurationTypeName, StringComparison.CurrentCulture));

    var implementationTypeName = $"{kvp.Key}{nameof(ICookiePersister).Substring(1)}";
    var implementationType = scannedAssemblies
      .Types
      .SingleOrDefault(x => x.Name.Equals(implementationTypeName, StringComparison.CurrentCulture));

    if (configurationType == null || configurationType == typeof(NoOpCookiePersisterConfig))
    {
      services.AddSingleton<ICookiePersister, NoOpCookiePersister>();
      return;
    }

    if (implementationType == null)
      throw new InvalidOperationException(
        $"Unable to find {nameof(ICookiePersister)} implementation `{implementationTypeName}`");
      
    var details = Activator.CreateInstance(configurationType);
    sub.Bind(kvp.Key, details);
    if (details == null)
      throw new InvalidOperationException(
        $"Unable to convert `{CookiePersisterConfig.SectionName}` to target type `{configurationType.FullName}`");

    services.AddSingleton(configurationType, details);
    services.AddSingleton(typeof(ICookiePersister), implementationType);
  }

  protected override void OnStartBeforeTest(ITest test)
  {
    scannedAssemblies = new AssemblyScanner(TestContext.CurrentContext.TestDirectory) { ThrowExceptions = false }
      .GetScannableAssemblies();

    /* these should be set by another ITestAction and should be prior to this one on the fixture */
    downloadRequired = test.Fixture is IRequireDownloadDirectory;
    screenshotsRequired = test.Fixture is IRequireScreenshots;
    
    var requireSiteType = test.Fixture!
      .GetType()
      .GetInterfaces()
      .FirstOrDefault(x => x.IsGenericType && RequiresWebType.IsAssignableFrom(x.GetGenericTypeDefinition()));

    if (requireSiteType != null)
    {
      var startPageType = requireSiteType.GetGenericArguments()[0];
      websiteType = typeof(Landing<>).MakeGenericType(startPageType);
      websiteInitialize = websiteType.GetMethod(nameof(Landing<DummyPage>.Initialize))!;
      websiteSetProperty = requireSiteType.GetProperty("Site", websiteType)!.SetMethod!;

      webPageDescriptors = scannedAssemblies
        .Types
        .Where(x => !x.IsAbstract && x.BaseType != null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition().IsAssignableTo(typeof(Page<>)))
        .Select(x => (Type: x, Generic: x.BaseType!.GetGenericTypeDefinition(), Arg: x.BaseType!.GetGenericArguments()))
        .Select(x => new ServiceDescriptor(typeof(WebPage), x.Type, ServiceLifetime.Scoped))
        .ToArray();

      websiteRequired = true;
    }
  }

  protected override void OnBeforeCreateHost(IHostBuilder builder, ITest test) { }

  protected override void OnAfterCreateHost(ITest test)
  {
    screenshotOptions = GetSetting<ScreenshotOptions?>("selenium:screenshots") ?? ScreenshotOptions.Default;
    seleniumScope = Host.Services.CreateScope();
    config = seleniumScope.ServiceProvider.GetRequiredService<WebDriverConfig>();
    fixture.Driver = driver = seleniumScope.ServiceProvider.GetRequiredService<IWebDriverExtensions>();
    cookiePersister = seleniumScope.ServiceProvider.GetRequiredService<ICookiePersister>();

    if (websiteRequired)
    {
      website = (WebSite)seleniumScope.ServiceProvider.GetRequiredService(websiteType!);
      websiteSetProperty.Invoke(test.Fixture, new object?[] { website });
    }
  }

  protected override void OnEndBeforeTest(ITest test)
  {
    if (test.Fixture is IConfigureWebDriverConfig wdc)
      wdc.Configure(config);

    if (test.Fixture is IRequireAuthentication && !cookiePersister.Initialized)
    { 
      /* test should fail anyway, but lets log it */
      this.LoggerFactory!.CreateLogger<RequireSeleniumAttribute>()
        .LogWarning($"fixture implements `{nameof(IRequireAuthentication)}` but no cookies found.");
    }
    else if (test.Fixture is IRequireAuthentication)
    {
      /* this assumes a user has somehow persisted the cookies; we don't automate that */
      var container = cookiePersister.Retrieve().GetAwaiter().GetResult();
      var path =
        config.Authentication.AnonymousPath.StartsWith("/", StringComparison.CurrentCulture)
          ? config.Authentication.AnonymousPath.Substring(1)
          : config.Authentication.AnonymousPath;

      driver.Navigate().GoToUrl(new Uri(config.BaseUri) + path);
      //driver.DefinitivelyWaitForAnyAjax();
      driver.ImportCookies(container, new Uri(config.BaseUri));
    }

    if (websiteRequired)
    {
      websiteInitialize.Invoke(website, Array.Empty<object?>());
    }
    else
    {
      driver.Navigate().GoToUrl(config.BaseUri);
    }

    var screenshots = screenshotOptions switch {
      ScreenshotOptions.FailuresAtEnd => false,
      ScreenshotOptions.Suppress      => false,
      _                               => screenshotsRequired
    };

    if (screenshots)
    {
      SafelyTry.Action(() => SaveScreenshotForEachTab(CleanPath($"{test.Name}-Pre")));
    }
  }

  protected override void OnStartAfterTest(ITest test)
  {
    var screenshots = (screenshotOptions, TestContext.CurrentContext.Result.Outcome.Status) switch {
      (ScreenshotOptions.Suppress, _) => false,
      _                               => screenshotsRequired
    };

    if (screenshots)
    {
      SafelyTry.Action(() => SaveScreenshotForEachTab(CleanPath($"{test.Name}-Post")));
    }

    SafelyTry.Dispose(() => seleniumScope);
  }

  private void SaveScreenshotForEachTab(string fileName)
  {
    if (driver == null)
      return;

    var path = $"{driver.Runtime.ScreenshotDirectory}\\{fileName}";
    driver.SaveScreenshotForEachTab(path);

    if (!string.IsNullOrEmpty(path))
    {
      Console.WriteLine($"##vso[artifact.upload artifactname={fileName}]{path}");
    }
  }

  private static string CleanPath(string path)
  {
    var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
    var r = new Regex($"[{Regex.Escape(regexSearch)}]");
    return r.Replace(path, string.Empty);
  }
  
  private class DummyPage : Page<DummyPage>
  {
    public DummyPage(IWebDriverExtensions driver) : base(driver) { }
  }
}
