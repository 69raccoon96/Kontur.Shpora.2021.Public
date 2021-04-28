using System;
using System.Collections.Generic;
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

        public override Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = new List<Task<string>>();
            foreach (var server in servers)
            {
                tasks.Add(Task<string>.Factory.StartNew(() =>
                {
                    var webRequest = Utilities.CreateRequest(server + "?query=" + query);
                    var res = ProcessRequestAsync(webRequest);
                    return res.Result;
                }));
            }

            var index = Task.WaitAny(tasks.ToArray(), timeout);
            
            var res = tasks.FirstOrDefault(x => x.IsCompletedSuccessfully);
            if(res == null)
                throw new TimeoutException();
            return Task.FromResult( res.Result);
        }

        public override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
