using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ClusterClient.Clients
{
    public class Utilities
    {
        public static HttpWebRequest CreateRequest(string uriStr)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        public static async Task<RequestResult> PingAddrAsync(IPAddress ipAddr, int timeout = 3000)
        {
            using var ping = new Ping();
            var res = await ping.SendPingAsync(ipAddr, timeout);
            return new RequestResult(ipAddr.ToString(), res.Status == IPStatus.Success, default,
                new TimeSpan(res.RoundtripTime));
        }
    }
}