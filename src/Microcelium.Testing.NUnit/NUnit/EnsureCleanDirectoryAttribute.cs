using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
  public class EnsureCleanDirectoryAttribute : Attribute, ITestAction
  {
    private readonly string directory;
    private readonly bool useTestContext;

    public EnsureCleanDirectoryAttribute(
      string directory,
      bool useTestContext = true,
      ActionTargets target = ActionTargets.Default)
    {
      this.directory = directory;
      this.useTestContext = useTestContext;
      Targets = target;
    }

    public void BeforeTest(ITest testDetails)
    {
      var path = useTestContext ? Path.Combine(directory, testDetails.Fixture.GetType().Name) : directory;
      var directoryInfo = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, path));

      if (directoryInfo.Exists)
      {
        directoryInfo.Delete(true);
        var count = 0;
        while (directoryInfo.Exists)
        {
          directoryInfo.Refresh();
          if (++count == 5)
            break;
        }
      }

      directoryInfo.Create();
      OnDirectoryCreated(testDetails, directoryInfo.FullName);
    }

    public void AfterTest(ITest testDetails) { }

    public ActionTargets Targets { get; }

    protected virtual void OnDirectoryCreated(ITest testDetails, string createdDirectory) { }
  }
}