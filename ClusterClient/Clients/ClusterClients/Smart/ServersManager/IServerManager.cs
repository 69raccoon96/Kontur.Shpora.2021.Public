using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClusterClient.Clients.ServersManager
{
    public interface IServerManager
    {
        public int ServersCount { get; }
        public string GetBestServer();

        public void Restart();

    }
}