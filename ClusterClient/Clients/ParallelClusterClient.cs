﻿using System;
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
    public class ParallelClusterClient : ClusterClientBase
    {
        private string[] servers;
        public ParallelClusterClient(ILog log, string[] replicaAddresses) : base(replicaAddresses)
        {
            servers = replicaAddresses;
            Log = log;
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = servers.Select(server => Utilities.CreateRequest(server + "?query=" + query))
                .Select(ProcessRequestAsync).ToList();
            while (true)
            {
                var res = await Task.Factory.StartNew(() => Task.WaitAny(tasks.ToArray(), timeout));
                if (res == -1)
                    break;
                
                if(tasks[res].IsCompletedSuccessfully)
                    return tasks[res].Result;
                tasks.Remove(tasks[res]);
            }
            throw new TimeoutException();
        }
        public override ILog Log { get; }
    }
}