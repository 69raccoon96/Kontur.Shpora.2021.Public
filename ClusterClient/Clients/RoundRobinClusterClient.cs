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
    public class RoundRobinClusterClient : ClusterClientBase
    {
        private readonly string[] _servers;

        public RoundRobinClusterClient(ILog log, string[] replicaAddresses) : base(replicaAddresses)
        {
            this._servers = replicaAddresses;
            Log = log;
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var delta =  timeout.Divide(_servers.Length);
            var goodServers = _servers.Length;
            var sw = Stopwatch.StartNew();
            foreach (var request in _servers.Select(x => CreateRequest(x + "?query=" + query)))
            {
                var task = ProcessRequestAsync(request);

                var currentIndex = await Task.Factory.StartNew(() => Task.WaitAny(new Task[] {task}, delta));

                if (currentIndex != -1)
                {
                    if(task.IsCompletedSuccessfully)
                        return task.Result;
                    goodServers--;
                    delta = timeout.Subtract(sw.Elapsed).Divide(goodServers);
                }
            }

            throw new TimeoutException();
        }

        public override ILog Log { get; }
    }
}