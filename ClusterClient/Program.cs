using ClusterClient.Clients;
using ClusterClient.Clients.ArgumentParsers;
using ClusterClient.Clients.ServersManager;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace ClusterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var containerBuilder = new ServiceCollection();
            containerBuilder.AddSingleton<IClient, SmartClusterClient>();
            containerBuilder.AddSingleton<IArgumentParser, FCLArgumentParser>();
            containerBuilder.AddSingleton<ContainerWorker>();
            containerBuilder.AddSingleton<IServerManager, ServersManager>();
            containerBuilder.AddSingleton(typeof(string[]), args);
            containerBuilder.AddSingleton(typeof(ILog), LogManager.GetLogger(typeof(Program)));
            var container = containerBuilder.BuildServiceProvider();
            container.GetService<ContainerWorker>().DoWork();
        }
    }
}