
namespace Microcelium.Testing.Web;

public interface IRequireWebHost : IRequireHost
{
  Uri HostUri { get; set; }
}

public interface IConfigureWebHostAddress : IRequireWebHost
{
  string GetHostUri();
}