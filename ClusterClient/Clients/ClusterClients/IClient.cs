using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public interface IClient
    {

        public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
        public abstract ILog Log { get; }
        //public Task<string> ProcessRequestAsync(WebRequest request);
    }
}