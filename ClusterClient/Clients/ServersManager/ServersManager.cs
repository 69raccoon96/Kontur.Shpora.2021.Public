using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClusterClient.Clients;
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
            UpdateServers();
        }


        public string GetBestServer()
        {
            if (CurrentIndex == SortedServers.Length)
                CurrentIndex = 0;
            return SortedServers[CurrentIndex++].Name;
        }

        private void UpdateServers()
        {
            
            foreach (var requestResult in GetServersRequests())
                _servers[requestResult.ServerName].UpdateInfo(requestResult.IsSuccess, requestResult.Time);

            SortedServers = SortedServers.Select(x => x)
                .OrderBy(x => x.FailedTimes)
                .ThenBy(x => x.AverageTimeResponse)
                .ToArray();
        }

        private List<RequestResult> GetServersRequests()
        {
            var requests = new List<RequestResult>();
            for (var i = 0; i < 3; i++)
            {
                foreach (var server in _serversAddresses)
                {
                    var resultPing = Utilities.PingAddrAsync(IPAddress.Parse(server)).Result;
                    requests.Add(resultPing);
                }
            }
            return requests;
        }
    }
}