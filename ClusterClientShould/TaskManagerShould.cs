using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClusterClient;
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
                "http://127.0.0.1:8060/qqq/",
                "http://127.0.0.1:8010/qqq/",
                "http://127.0.0.1:8020/qqq/"
            };
            _serversManager = new ServersManager(servers);
        }

        [Test]
        public void GetBestServer_CorrectSortedByFailedTimes()
        {
            var expectedServer = "http://127.0.0.1:8060/qqq/";
            var requestResults = new List<Task<RequestResult>>
            {
                Task.FromResult(new RequestResult("http://127.0.0.1:8010/qqq/")),
                Task.FromResult(new RequestResult("http://127.0.0.1:8060/qqq/", true, default, new TimeSpan(10))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8020/qqq/")),
                Task.FromResult(new RequestResult("http://127.0.0.1:8010/qqq/", true,default, new TimeSpan(1))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8020/qqq/", true, default, new TimeSpan(1)))
            };
            
            _serversManager.UpdateServers(requestResults);
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
            var requestResults = new List<Task<RequestResult>>
            {
                Task.FromResult(new RequestResult("http://127.0.0.1:8010/qqq/", true, default, new TimeSpan(30))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8010/qqq/",true, default, new TimeSpan(40))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8060/qqq/", true,default, new TimeSpan(100))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8020/qqq/", true, default, new TimeSpan(20))),
                Task.FromResult(new RequestResult("http://127.0.0.1:8020/qqq/", true, default, new TimeSpan(60)))
            };

            _serversManager.UpdateServers(requestResults);
            var actualBestServer = _serversManager.GetBestServer();

            expectedServer.Should().BeSameAs(actualBestServer);
        }

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