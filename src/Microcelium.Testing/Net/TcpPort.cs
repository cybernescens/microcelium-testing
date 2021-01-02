using System.Net;
using System.Net.Sockets;

namespace Microcelium.Testing.Net
{
  public static class TcpPort
  {
    private static readonly object lockObject = new object();

    public static int NextFreePort()
    {
      lock (lockObject)
      {
        var l = new TcpListener(IPAddress.Loopback, 0);
        try
        {
          l.Start();

          return ((IPEndPoint)l.LocalEndpoint).Port;
        }
        finally
        {
          l.Stop();
        }
      }
    }
  }
}