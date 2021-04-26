using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Clients;
using log4net;

namespace ClusterClient
{
    public class SmartClusterClient : ClusterClientBase
    {
        private readonly ServersManager _serversManager;
        private readonly int _serversCount;
        protected override ILog Log { get; }

        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            _serversManager = new ServersManager(replicaAddresses);
            _serversCount = replicaAddresses.Length;
        }

        public async override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = new List<Task<RequestResult>>();
            var delta = (int) timeout.TotalMilliseconds / (_serversCount + 1);
            var pull = timeout.Seconds * 1000;
            var startedServers = 0;
            while (startedServers != _serversCount)
            {
                var pull1 = pull;
                var task = new Task<RequestResult>(() => CreateTask(query, pull1).Result);
                task.Start();
                tasks.Add(task);
                pull -= delta;
                startedServers++;
                Thread.Sleep(delta);
                if (tasks.Any(x => x.Status == TaskStatus.RanToCompletion))
                    break;
            }

            Thread.Sleep(delta);
            new Task(() => _serversManager.UpdateServers(tasks)).Start();

            var completedTask = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully);
            if (completedTask == null)
                throw new TimeoutException();
            return completedTask.Result.Result;
        }

        private async Task<RequestResult> CreateTask(string query, int timeout)
        {
            
            var server = _serversManager.GetBestServer();
            var token = new CancellationTokenSource();
            try
            {
                token.CancelAfter(timeout);
                /*var webRequest = CreateRequest(server + "?query=" + query);

                Log.InfoFormat($"Processing {webRequest.RequestUri}");*/
                var sw = Stopwatch.StartNew();
                //var reqResult = await ProcessRequestAsync(webRequest);
                var reqResult = await ProcessRequestAsync2(server); 
                Console.WriteLine("I was here");
                return await Task.FromResult(new RequestResult(server, true, reqResult, new TimeSpan(sw.ElapsedTicks)));
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nTasks cancelled: timed out.\n");
                return await Task.FromResult(new RequestResult(server));
            }
            finally
            {
                token.Dispose();
            }
        }

        private Task<string> ProcessRequestAsync2(string request)
        {
            var time = new Random().Next(2000, 6000);
            Console.WriteLine("Сервер: " + request + " Время сна: " + time);
            Thread.Sleep(time);
            return Task.FromResult("ok");
        }
    }
}