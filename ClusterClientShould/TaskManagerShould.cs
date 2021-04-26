using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClusterClient;
using ClusterClient.Clients.ArgumentParsers;
using NUnit.Framework;
using FluentAssertions;

namespace ClusterClientShould
{
    public class TaskManagerShould
    {
        private ServersManager _serversManager;

        [SetUp]
        public void Setup()
        {
            var servers = new[]
            {
                "87.250.250.242",
                "172.217.18.110",
            };
            _serversManager = new ServersManager(new FCLArgumentParser(), new [] {"ServerAddresses.txt"});
        }

        [Test]
        public void Constructor_CorrectSortServers()
        {
            _serversManager.GetBestServer();
        }
        

        /*[Test]
        public void GetBestServer_CorrectSortedByFailedTimes()
        {
            var expectedServer = "http://127.0.0.1:8060/qqq/";
            
            
            //_serversManager.UpdateServers(requestResults);
            var actualBestServer = _serversManager.GetBestServer();

            expectedServer.Should().BeSameAs(actualBestServer);
        }

        [Test]
        public void GetBestServer_ThrowExceptionWhenNoMoreServers()
        {
            _serversManager.GetBestServer();
            _serversManager.GetBestServer();
            _serversManager.GetBestServer();
            Action action = () => _serversManager.GetBestServer();

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("No more servers (Parameter 'ServerIndex')");
        }

        [Test]
        public void GetBestServer_CorrectSortedByAverageTime()
        {
            var expectedServer = "http://127.0.0.1:8010/qqq/";
            
            var actualBestServer = _serversManager.GetBestServer();

            expectedServer.Should().BeSameAs(actualBestServer);
        }*/

        /*[Test]
        public void GetBestServer_CorrectSortedByAverageTimeAndFailedTimes()
        {
            var expectedServer = "http://127.0.0.1:8010/qqq/";
                
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", true , new TimeSpan(30));
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", true , new TimeSpan(40));
            _tasksManager.UpdateServer("http://127.0.0.1:8060/qqq/", true , new TimeSpan(100));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", true , new TimeSpan(20));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", true , new TimeSpan(60));
            var actualBestServer = _tasksManager.GetBestServer();

            expectedServer.Should().BeSameAs(actualBestServer);
        }*/
    }
}