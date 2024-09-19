using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class NetTools
    {
        /// <summary>
        /// get current binds ip v4
        /// </summary>
        /// <returns></returns>
        public static string[] GetIPv4s()
        {
            var ips = Dns.GetHostAddresses(Dns.GetHostName());
            return ips.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToArray();
        }

        /// <summary>
        /// add all ip to prefixes
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="port"></param>
        public static void AddHttpPrefixes(HttpListener listener, int port)
        {
            var ips = NetTools.GetIPv4s();
            foreach (var ip in ips)
            {
                listener.Prefixes.Add($"http://{ip}:{port}/");
            }
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }
    }
}
