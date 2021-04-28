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
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            servers = replicaAddresses;
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
        
        private async Task<string> CreateTask(string query, string server)
        {
            var webRequest = Utilities.CreateRequest(server + "?query=" + query);
            Log.InfoFormat($"Processing {webRequest.RequestUri}");
            try
            {   
                Console.WriteLine($"Await answer from {server}");
                var reqResult = await ProcessRequestAsync(webRequest);
                //var reqResult = await ProcessRequestAsync2(server); 
                return reqResult;
            }
            catch
            {
                throw new TimeoutException();
            }
        }
        private async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using var response = await request.GetResponseAsync();
            var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
            Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
            return result;
        }

        public override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}