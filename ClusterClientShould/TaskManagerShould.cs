using System;
using ClusterClient;
using NUnit.Framework;
using FluentAssertions;

namespace ClusterClientShould
{
    public class TaskManagerShould
    {
        private TasksManager _tasksManager;
        [SetUp]
        public void Setup()
        {
            var servers = new[]
            {
                "http://127.0.0.1:8060/qqq/",
                "http://127.0.0.1:8010/qqq/",
                "http://127.0.0.1:8020/qqq/"
            };
            _tasksManager = new TasksManager(servers);
        }
        [Test]
        public void GetBestServer_CorrectSortedByFailedTimes()
        {
            var expectedServer = "http://127.0.0.1:8060/qqq/";
                
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", false);
            _tasksManager.UpdateServer("http://127.0.0.1:8060/qqq/", true , new TimeSpan(10));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", false);
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", true, new TimeSpan(1));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", true, new TimeSpan(1));
            var actualBestServer = _tasksManager.GetBestServer();

            expectedServer.Should().BeSameAs(actualBestServer);
        }

        [Test]
        public void GetBestServer_ThrowExceptionWhenNoMoreServers()
        {
            _tasksManager.GetBestServer();
            _tasksManager.GetBestServer();
            _tasksManager.GetBestServer();
            Action action = () => _tasksManager.GetBestServer();

            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage("No more servers (Parameter 'ServerIndex')");
        }
        
        [Test]
        public void GetBestServer_CorrectSortedByAverageTime()
        {
            var expectedServer = "http://127.0.0.1:8010/qqq/";
                
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", true , new TimeSpan(30));
            _tasksManager.UpdateServer("http://127.0.0.1:8010/qqq/", true , new TimeSpan(40));
            _tasksManager.UpdateServer("http://127.0.0.1:8060/qqq/", true , new TimeSpan(100));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", true , new TimeSpan(20));
            _tasksManager.UpdateServer("http://127.0.0.1:8020/qqq/", true , new TimeSpan(60));
            var actualBestServer = _tasksManager.GetBestServer();

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