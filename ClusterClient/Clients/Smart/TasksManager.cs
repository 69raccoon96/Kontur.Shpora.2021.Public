using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterClient
{
    public class TasksManager
    {
        private ServerData[] _sortedServers;
        private readonly Dictionary<string, ServerData> _servers; 
        private int _currentIndex;

        public TasksManager(IReadOnlyCollection<string> servers)
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

        public void UpdateServer(string name, bool isSuccess, TimeSpan time = default)
        {
            _currentIndex = 0;
            _servers[name].UpdateInfo(isSuccess, time);
            _sortedServers = _sortedServers
                .Select(x => x)
                .OrderBy(x => x.FailedTimes)
                .ThenBy(x => x.AverageTimeResponse)
                .ToArray();
        }
    }
}