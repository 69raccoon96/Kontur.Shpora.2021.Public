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
    public class SmartClusterClient : IClient
    {
        private readonly IServerManager _serversManager;
        public ILog Log { get; }

        public SmartClusterClient(IServerManager serverManager, ILog log)
        {
            _serversManager = serverManager;
            Log = log;
        }

        public Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var serversCount = _serversManager.ServersCount;
            var tasks = new List<Task<RequestResult>>();
            var delta = (int) timeout.TotalMilliseconds / (serversCount + 1);
            var timeToAdd = 0;
            var startedTasks = 0;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                if (sw.ElapsedMilliseconds < timeToAdd || startedTasks == _serversManager.ServersCount) continue;
                if (tasks.Any(x => x.Status == TaskStatus.RanToCompletion))
                    break;
                timeToAdd += delta;
                var task = new Task<RequestResult>(() => CreateTask(query).Result);
                task.Start();
                startedTasks++;
                tasks.Add(task);
            }

            _serversManager.UpdateServers(tasks);
            var completedTask = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully);
            Console.WriteLine("I finished: " + sw.ElapsedMilliseconds);
            
            if (completedTask == null)
                throw new TimeoutException();
            return Task.FromResult(completedTask.Result.Result);
        }

        private async Task<RequestResult> CreateTask(string query)
        {
            var server = _serversManager.GetBestServer();
            var webRequest = Utilities.CreateRequest(server + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            var sw = Stopwatch.StartNew();
            try
            {
               //var reqResult = await ProcessRequestAsync(webRequest);
               var reqResult = await ProcessRequestAsync2(server); 
               return new RequestResult(server, true, reqResult, new TimeSpan(sw.ElapsedTicks));
            }
            catch
            {
                return new RequestResult(server);
            }

        }

        private Task<string> ProcessRequestAsync2(string request)
        {
            var time = new Random().Next(500, 11500);
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