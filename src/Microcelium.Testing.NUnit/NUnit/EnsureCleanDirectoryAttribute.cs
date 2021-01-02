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

    public EnsureCleanDirectoryAttribute(string directory, bool useTestContext = true, ActionTargets target = ActionTargets.Default)
    {
      this.directory = directory;
      this.useTestContext = useTestContext;
      Targets = target;
    }

    public void BeforeTest(ITest testDetails)
    {
      var path = useTestContext ? Path.Combine(directory, testDetails.Fixture.GetType().Name) : directory;
      
      //Log.Info("Cleaning '{0}' directory...", path);

      var directoryInfo = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, path));
      //Log.Info("Checking if directory '{0}' exists...", directoryInfo);
      
      if (directoryInfo.Exists)
      {
        //Log.Info("Directory '{0}' exists so deleting", directoryInfo);
        directoryInfo.Delete(true);
        var count = 0;
        while (directoryInfo.Exists)
        {
          directoryInfo.Refresh();
          if (++count == 5)
            break;
        }

        //Log.Info("Directory deleted");
      }

      //Log.Info("Creating directory '{0}'", directoryInfo);
      directoryInfo.Create();
      // Log.Info("Directory created");

      OnDirectoryCreated(testDetails, directoryInfo);

      //Log.Info("Directory '{0}' cleaned", path);
    }

    public void AfterTest(ITest testDetails) { }

    public ActionTargets Targets { get; }

    protected virtual void OnDirectoryCreated(ITest testDetails, DirectoryInfo createdDirectory) { }
  }
}