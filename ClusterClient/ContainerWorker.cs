using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Clients;
using ClusterClient.Clients.ArgumentParsers;
using log4net;
using log4net.Config;

namespace ClusterClient
{
    public class ContainerWorker
    {
        private readonly ILog _log;
        private readonly IClient _client;

        public ContainerWorker(ILog log, IClient client)
        {
            _log = log;
            _client = client;
        }
        
        public void DoWork()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo("log4net.config"));
            
            try
            {
                var queries = new[]
                {
                    "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
                    "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
                    "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
                };

                Console.WriteLine("Testing {0} started", _client.GetType());
                Task.WaitAll(queries.Select(
                    async query =>
                    {
                        var timer = Stopwatch.StartNew();
                        try
                        {
                            await _client.ProcessRequestAsync(query, TimeSpan.FromSeconds(6));

                            Console.WriteLine("Processed query \"{0}\" in {1} ms", query,
                                timer.ElapsedMilliseconds);
                        }
                        catch (TimeoutException)
                        {
                            Console.WriteLine("Query \"{0}\" timeout ({1} ms)", query, timer.ElapsedMilliseconds);
                        }
                    }).ToArray());
                Console.WriteLine("Testing {0} finished", _client.GetType());
            }
            catch (Exception e)
            {
                _log.Fatal(e);
                Console.WriteLine($"E :{e}");
            }
        }
    }
}