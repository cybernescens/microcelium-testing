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
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium;

public class RequireSeleniumAttribute : RequireHostAttribute
{
  private static readonly Type RequiresWebType = typeof(IRequireWebSite<>);
  
  private string? downloadDirectory;
  private bool authenticationRequired;
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

    services.AddSingleton<WebDriverFactory>();

    services.AddScoped(_ => new RuntimeConfig {
      DownloadDirectory = downloadDirectory,
      ScreenshotDirectory = screenshotsDirectory
    });

    RegisterCookiePersister(ctx, services);

    if (config.Authentication.IsLocalCredentials())
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

    if (authenticationRequired)
    {
      services.AddScoped(
        sp => {
          var cfg = sp.GetRequiredService<WebDriverConfig>();
          var runtime = sp.GetRequiredService<RuntimeConfig>();
          var d = sp.GetRequiredService<WebDriverFactory>().Create(runtime);
          var jar = d.Manage().Cookies;
          jar.DeleteAllCookies();
          
          var persister = sp.GetRequiredService<ICookiePersister>();
          var container = persister.Retrieve().GetAwaiter().GetResult();

          container
            .GetCookies(new Uri(cfg.BaseUri))
            .Select(
              c => {
                c.Domain = c.Domain.Contains("localhost") ? null : c.Domain;
                return c;
              })
            .ToList()
            .ForEach(cookie => jar.AddCookie(
              new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, null)));

          return d;
        });
    }
    else
    {
      services.AddScoped(
        sp => {
          var runtime = sp.GetRequiredService<RuntimeConfig>();
          return sp.GetRequiredService<WebDriverFactory>().Create(runtime);
        });
    }

    services.AddScoped<IWebDriverExtensions, WebDriverAdapter>();

    if (websiteRequired && websiteType != null)
    {
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
      
    var details = sub.GetValue(configurationType, kvp.Key);
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
    if (test.Fixture is IRequireDownloadDirectory dd)
    {
      downloadRequired = true;
      downloadDirectory = dd.DownloadDirectory;
    }

    if (test.Fixture is IRequireScreenshots ss)
    {
      screenshotsRequired = true;
      screenshotsDirectory = ss.ScreenshotDirectory;
    }
    
    authenticationRequired = test.Fixture is IRequireAuthentication;

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

  protected override void OnHostBuilding(IHostBuilder builder, ITest test)
  {
    if (test.Fixture is IConfigureSeleniumWebDriverConfig wdc)
      wdc.Configure(config);
  }

  protected override void OnHostBuilt(ITest test)
  {
    screenshotOptions = GetSetting<ScreenshotOptions?>("selenium:screenshots") ?? ScreenshotOptions.Default;
    config = serviceScope!.ServiceProvider.GetRequiredService<WebDriverConfig>();
    fixture.Driver = driver = serviceScope!.ServiceProvider.GetRequiredService<IWebDriverExtensions>();
    cookiePersister = serviceScope!.ServiceProvider.GetRequiredService<ICookiePersister>();

    if (websiteRequired)
    {
      website = serviceScope!.ServiceProvider.GetRequiredService<WebSite>();
      websiteSetProperty.Invoke(test.Fixture, new object?[] { website });
    }
  }

  protected override void ApplyToContext()
  {
    AddToContext(nameof(ICookiePersister), cookiePersister);
    AddToContext(nameof(WebDriverConfig), config);
    AddToContext(nameof(IWebDriverExtensions), driver);
    
    if (websiteRequired && website != null)
      AddToContext(nameof(WebSite), website);
  }

  protected override void OnEndBeforeTest(ITest test)
  {
    if (authenticationRequired && !cookiePersister.Initialized)
    { 
      /* test should fail anyway, but lets log it */
      loggerFactory!.CreateLogger<RequireSeleniumAttribute>()
        .LogWarning($"fixture implements `{nameof(IRequireAuthentication)}` but no cookies found.");
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
      SafelyTry.Action(
        () => SaveScreenshotForEachTab(((IRequireScreenshots)test.Fixture!).ScreenshotDirectory!, "-Pre"));
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
      var dir = ((IRequireScreenshots)test.Fixture!).ScreenshotDirectory!;
      SafelyTry.Action(() => SaveScreenshotForEachTab(dir, "-Post"));
    }
  }

  private void SaveScreenshotForEachTab(string directory, string suffix)
  {
    if (driver == null)
      return;

    var fileName = CleanPath($"{TestContext.CurrentContext.Test.FullName}{suffix}.png");
    var path = $"{directory}\\{fileName}";
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
