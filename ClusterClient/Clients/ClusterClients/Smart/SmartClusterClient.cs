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

        public SmartClusterClient(IServerManager serverManager, ILog log) : base(null)
        {
            _serversManager = serverManager;
            Log = log;
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            
            var serversCount = _serversManager.ServersCount;
            var tasks = new List<Task<RequestResult>>();
            var delta = (int) timeout.TotalMilliseconds / serversCount;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                var task = new Task<RequestResult>(() => CreateTask(query));
                task.Start();
                tasks.Add(task);
                Task.WaitAny(tasks.ToArray(), TimeSpan.FromMilliseconds(delta));
                if (tasks.Any(x => x.Status == TaskStatus.RanToCompletion && x.Result.IsSuccess))
                    break;
            }
            var completedTask = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully && x.Result.IsSuccess);
            _serversManager.Restart();
            if(completedTask?.Result.Result == null)
                throw new TimeoutException();
            return completedTask.Result.Result;

        }

        private  RequestResult CreateTask(string query)
        {
            var server = _serversManager.GetBestServer();
            var webRequest = Utilities.CreateRequest(server + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            var sw = Stopwatch.StartNew();
            try
            {   
                Console.WriteLine($"Await answer from {server}");
                var reqResult = ProcessRequestAsync(webRequest);
               //var reqResult = await ProcessRequestAsync2(server); 
               return new RequestResult(server, true, reqResult.Result, new TimeSpan(sw.ElapsedTicks));
            }
            catch
            {
                return new RequestResult(server);
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