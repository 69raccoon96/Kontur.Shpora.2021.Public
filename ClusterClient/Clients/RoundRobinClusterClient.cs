using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : IClient
    {
        private readonly string[] _servers;

        public RoundRobinClusterClient(string[] replicaAddresses)
        {
            this._servers = replicaAddresses;
        }

        public async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var index = 0;
            var delta =  timeout.Divide(_servers.Length);
            var goodServers = _servers.Length;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                var currentServer = _servers[index];
                var task = Task.Factory.StartNew(() => CreateTask(query, currentServer).Result);

                var currentIndex = await Task.Factory.StartNew(() => Task.WaitAny(new Task[] {task}, delta));

                if (currentIndex != -1 )
                {
                    if(task.IsCompletedSuccessfully)
                        return task.Result;
                    goodServers--;
                    delta = timeout.Subtract(sw.Elapsed).Divide(goodServers);
                }

                index++;
            }

            throw new TimeoutException();
        }

        private async Task<string> CreateTask(string query, string server)
        {
            var webRequest = Utilities.CreateRequest(server + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            try
            {
                Console.WriteLine($"Await answer from {server}");
                var reqResult = await ProcessRequestAsync(webRequest);
                return reqResult;
            }
            catch
            {
                throw new TimeoutException();
            }
        }

        private async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using var response = await request.GetResponseAsync();
            var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
            Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
            return result;
        }

        public ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}