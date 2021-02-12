using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit
{
  public class EnsureCleanDownloadDirectoryAttribute : EnsureCleanDirectoryAttribute
  {
    public EnsureCleanDownloadDirectoryAttribute(
      string directory,
      bool useTestContext = true,
      ActionTargets target = ActionTargets.Default)
      : base(directory, useTestContext, target) { }

    protected override void OnDirectoryCreated(ITest testDetails, string createdDirectory)
    {
      if (testDetails.Fixture is IRequireDownloadDirectory requireDownloadDirectory)
        requireDownloadDirectory.DownloadDirectory = createdDirectory;
    }
  }
}