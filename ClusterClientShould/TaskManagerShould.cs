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
            _serversManager = new ServersManager(new FCLArgumentParser(), new [] {"-f:ServerAddresses.txt"});
        }

        [Test]
        public void Constructor_CorrectSortServers()
        {
            var expectedServer = "87.250.250.242";
            
            var actualServer = _serversManager.GetBestServer();

            expectedServer.Should().Be(actualServer);
        }
        
    }
}