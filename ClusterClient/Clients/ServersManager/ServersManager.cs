using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterClient.Clients.ArgumentParsers;
using ClusterClient.Clients.ServersManager;

namespace ClusterClient
{
    public class ServersManager : IServerManager
    {
        public int ServersCount { get; }
        private ServerData[] SortedServers { get; set;}
        private readonly Dictionary<string, ServerData> _servers;
        private readonly string[] _serversAddresses;
        private int CurrentIndex { get; set; }

        public ServersManager(IArgumentParser argumentParser, string[] args)
        {
            if(!argumentParser.TryGetReplicaAddresses(args, out var addresses))
                throw new Exception("no file");
            _serversAddresses = addresses;
            ServersCount = addresses.Length;
            SortedServers = new ServerData[addresses.Length];
            CurrentIndex = 0;
            _servers = new Dictionary<string, ServerData>();
            var index = 0;
            foreach (var server in addresses)
            {
                var serverToAdd = new ServerData(server);
                _servers.Add(server, serverToAdd);
                SortedServers[index] = serverToAdd;
                index++;
            }
        }


        public string GetBestServer()
        {
            if(CurrentIndex == SortedServers.Length)
                throw new ArgumentOutOfRangeException("ServerIndex","No more servers");
            return SortedServers[CurrentIndex++].Name;
        }

        public void UpdateServers(List<Task<RequestResult>> serverResults)
        {
            /*var notUpdatedServers = _serversAddresses.ToList();
            foreach (var serverResult in serverResults)
            {
                var requestResult = serverResult.Result;
                _servers[requestResult.ServerName].UpdateInfo(requestResult.IsSuccess, requestResult.Time);
                notUpdatedServers.Remove(requestResult.ServerName);

            }

            foreach (var notUpdatedServer in notUpdatedServers)
            {
                _servers[notUpdatedServer].UpdateInfo(false);
            }

            SortedServers = SortedServers.Select(x => x)
                .OrderBy(x => x.FailedTimes)
                .ThenBy(x => x.AverageTimeResponse)
                .ToArray();*/
            CurrentIndex = 0;
        }
    }
}