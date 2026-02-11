using System.Net;
using System.Net.Sockets;

namespace Zylance.Desktop.Lib;

public static class WebUtils
{
    /// <summary>
    ///     Discovers an available port by attempting to bind to it.
    /// </summary>
    /// <param name="min">The start of the range to search for a port</param>
    /// <param name="max">The end (inclusive) of the range to search for a port</param>
    /// <param name="maxAttempts">Maximum number of ports to try.</param>
    /// <returns>An available port number.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no available port is
    ///     found after max attempts.
    /// </exception>
    public static int DiscoverAvailablePort(int min, int? max = null, int maxAttempts = 10)
    {
        var random = new Random();

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var port = random.Next(min, max ?? min + 100);

            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                return port;
            }
            catch (SocketException)
            {
                if (attempt == maxAttempts - 1)
                    throw new InvalidOperationException(
                        $"Failed to find an available port after {maxAttempts} attempts"
                    );
            }
        }

        throw new InvalidOperationException("Failed to discover an available port");
    }
}
