using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : IClient
    {
        public RoundRobinClusterClient(string[] replicaAddresses) 
        {
        }

        public Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
