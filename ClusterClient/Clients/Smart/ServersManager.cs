using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterClient
{
    public class ServersManager
    {
        private ServerData[] _sortedServers;
        private readonly Dictionary<string, ServerData> _servers; 
        private int _currentIndex;

        public ServersManager(IReadOnlyCollection<string> servers)
        {
            _sortedServers = new ServerData[servers.Count];
            _currentIndex = 0;
            _servers = new Dictionary<string, ServerData>();
            var index = 0;
            foreach (var server in servers)
            {
                var serverToAdd = new ServerData(server);
                _servers.Add(server, serverToAdd);
                _sortedServers[index] = serverToAdd;
                index++;
            }
        }

        public string GetBestServer()
        {
            if(_currentIndex == _sortedServers.Length)
                throw new ArgumentOutOfRangeException("ServerIndex","No more servers");
            return _sortedServers[_currentIndex++].Name;
        }

        public void UpdateServers(List<Task<RequestResult>> serverResults)
        {
            foreach (var serverResult in serverResults)
            {
                var requestResult = serverResult.Result;
                _servers[requestResult.ServerName].UpdateInfo(requestResult.IsSuccess, requestResult.Time);
            }

            _sortedServers = _sortedServers.Select(x => x)
                .OrderBy(x => x.FailedTimes)
                .ThenBy(x => x.AverageTimeResponse)
                .ToArray();
        }
    }
}