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
        private readonly TasksManager _tasksManager;
        private readonly int _serversCount;
        private readonly CancellationTokenSource [] _tokens;
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
           _tasksManager = new TasksManager(replicaAddresses);
           _serversCount = replicaAddresses.Length;
           _tokens = new CancellationTokenSource [_serversCount];
           for (var i = 0; i < _serversCount; i++)
           {
               _tokens[i] = new CancellationTokenSource ();
           }
        }

        public override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = new List<Task<RequestResult>>();
            var currentTicks = 0L;
            var delta = timeout.Ticks / _serversCount;
            var sw = Stopwatch.StartNew();
            tasks.Add(CreateTask(query, timeout));
            while (currentTicks <= timeout.Ticks)
            {
                if (sw.ElapsedTicks >= delta)
                {
                    tasks.Add(CreateTask(query,timeout.Subtract(new TimeSpan(sw.ElapsedTicks))));
                    currentTicks += delta;
                    sw.Restart();
                }
                else
                {
                    var flag = false;
                    foreach (var task in tasks)
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                            flag = true;
                    }
                    if(flag)
                        break;
                }
            }

            foreach (var task in tasks)
            {
                var taskRes = task.Result;
                if (task.IsFaulted)
                {
                    _tasksManager.UpdateServer(taskRes.ServerName, false);
                }

                if (task.IsCompletedSuccessfully)
                {
                    _tasksManager.UpdateServer(taskRes.ServerName, true, taskRes.Time);
                }
            }

            var completedTask = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully);
            if(completedTask == null)
                throw new TimeoutException();
            return Task.FromResult(completedTask.Result.Result);
        }

        private async Task<RequestResult> CreateTask(string query, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            var server = _tasksManager.GetBestServer();
            var token = new CancellationTokenSource();
            try
            {
                token.CancelAfter(timeout);
                
                Console.WriteLine(server);
                var webRequest = CreateRequest(server + "?query=" + query);

                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var reqResult = await ProcessRequestAsync(webRequest);
                return new RequestResult(server, reqResult, new TimeSpan(sw.ElapsedTicks));
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nTasks cancelled: timed out.\n");
                return new RequestResult(server);
            }
            finally
            {
                token.Dispose();
            }
        }

        protected override ILog Log { get; }
    }
}