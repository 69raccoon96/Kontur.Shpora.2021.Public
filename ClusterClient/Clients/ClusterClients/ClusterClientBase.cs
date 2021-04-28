using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase : IClient
    {
        public string[] ReplicaAddresses { get; set; }

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
        }

        public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
        public abstract ILog Log { get; }

        protected static HttpWebRequest CreateRequest(string uriStr)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        protected async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using var response = await request.GetResponseAsync();
            var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
            Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
            return result;
        }

        protected string CreateTask(string query, string server)
        {
            var webRequest = Utilities.CreateRequest(server + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            try
            {   
                Console.WriteLine($"Await answer from {server}");
                var reqResult = ProcessRequestAsync(webRequest);
                //var reqResult = await ProcessRequestAsync2(server); 
                return reqResult.Result;
            }
            catch
            {
                throw new TimeoutException();
            }
        }
    }
}