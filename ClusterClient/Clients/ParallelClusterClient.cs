using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClientBase
    {
        private string[] servers;
        public ParallelClusterClient(string[] replicaAddresses) : base(null)
        {
            servers = replicaAddresses;
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = new List<Task<string>>();
            foreach (var server in servers)
            {
                tasks.Add(Task<string>.Factory.StartNew(() =>
                {
                    var webRequest = Utilities.CreateRequest(server + "?query=" + query);
                    return ProcessRequestAsync(webRequest).Result;
                }));
            }

            await Task.Delay(timeout);
            var res = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully);
            if (res != null)
                return res.Result;
            throw new TimeoutException();

        }

        public override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
