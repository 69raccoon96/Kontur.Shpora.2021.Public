using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Clients;
using ClusterClient.Clients.ServersManager;
using log4net;

namespace ClusterClient
{
    public class SmartClusterClient : ClusterClientBase
    {
        private readonly IServerManager _serversManager;
        private bool NoGoodServers = false;
        public override ILog Log { get; }

        public SmartClusterClient(ILog log, string[] replicaAddressees) : base(replicaAddressees)
        {
            _serversManager = new ServersManager(replicaAddressees);
            Log = log;
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var serversCount = _serversManager.ServersCount;
            var servers = _serversManager.ServersAddresses;
            var index = 0;
            var tasks = new List<Task<string>>();
            var delta = timeout.Divide(serversCount);
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                var index1 = index;
                var task = Task.Factory.StartNew(() => CreateTask(query, servers[index1]).Result);
                tasks.Add(task);
                index++;
                var currentIndex = await Task.Factory.StartNew(() =>Task.WaitAny(tasks.ToArray(), delta));

                if (currentIndex != -1)
                {
                    if (tasks[currentIndex].IsCompletedSuccessfully)
                        return tasks[currentIndex].Result;
                    tasks.RemoveAll(x => x.IsFaulted);
                }
                

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
               //var reqResult = await ProcessRequestAsync2(server); 
               return reqResult;
            }
            catch
            {
               throw new TimeoutException();
            }
        }

        private Task<string> ProcessRequestAsync2(string request)
        {
            var time = new Random().Next(500, 1000);
            Console.WriteLine("Сервер: " + request + " Время сна: " + time);
            Thread.Sleep(time);

            return Task.FromResult("ok");
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