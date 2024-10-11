namespace Microcelium.Testing.Selenium;

public interface IRequireDownloadDirectory : IRequireDirectory
{
  string? DownloadDirectory { get; set; }
}