using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RandomClusterClient : IClient
    {
        private readonly Random random = new Random();
        public ILog Log { get; }

        private string[] ReplicaAddresses;

        public RandomClusterClient(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
        }

        public async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var uri = ReplicaAddresses[random.Next(ReplicaAddresses.Length)];

            var webRequest = Utilities.CreateRequest(uri + "?query=" + query);
            
            Log.InfoFormat($"Processing {webRequest.RequestUri}");

            var resultTask = ProcessRequestAsync(webRequest);

            await Task.WhenAny(resultTask, Task.Delay(timeout));
            if (!resultTask.IsCompleted)
                throw new TimeoutException();

            return resultTask.Result;
        }

        private async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using var response = await request.GetResponseAsync();
            var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
            Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
            return result;
        }

        
    }
}