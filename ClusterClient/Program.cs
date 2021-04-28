using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Clients;
using ClusterClient.Clients.ArgumentParsers;
using ClusterClient.Clients.ServersManager;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;

namespace ClusterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo("log4net.config"));
            var log = LogManager.GetLogger(typeof(Program));
            var parser = new FCLArgumentParser();
            if (parser.TryGetReplicaAddresses(args, out var replicaAddresses)) ;
            var manager = new ServersManager(replicaAddresses);
            var client = new SmartClusterClient(log, replicaAddresses);
            try
            {
                var queries = new[]
                {
                    "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
                    "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
                    "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
                };
                
                Console.WriteLine("Testing {0} started", client.GetType());
                Task.WaitAll(queries.Select(
                    async query =>
                    {
                        var timer = Stopwatch.StartNew();
                        try
                        {
                            await client.ProcessRequestAsync(query, TimeSpan.FromSeconds(6));

                            Console.WriteLine("Processed query \"{0}\" in {1} ms", query,
                                timer.ElapsedMilliseconds);
                        }
                        catch (TimeoutException)
                        {
                            Console.WriteLine("Query \"{0}\" timeout ({1} ms)", query, timer.ElapsedMilliseconds);
                        }
                        catch (AggregateException ae)
                        {
                            Console.WriteLine("Query \"{0}\" caused error)", query);
                            foreach(var e in ae.Flatten().InnerExceptions)
                                log.Error(e.ToString());
                        }
                    }).ToArray());
                Console.WriteLine("Testing {0} finished", client.GetType());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error");
                log.Error(e.ToString());
            }
        }
    }
}