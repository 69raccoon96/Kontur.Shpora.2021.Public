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
                var task = Task.Factory.StartNew(() => CreateTask(query, servers[index1]));
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
        
    }
}