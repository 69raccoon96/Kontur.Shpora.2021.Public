using System;
using NUnit.Framework;
using ClusterClient;
using FluentAssertions;

namespace ClusterClientShould
{
    public class ServerDataShould
    {
        private ServerData _serverData;
        [SetUp]
        public void Setup()
        {
            _serverData = new ServerData(null);
        }
        
        [Test]
        public void UpdateInfo_CorrectUpdateAverageTimeWhenSuccess()
        {
            var expectedTimeSpan = new TimeSpan(15);
            _serverData.UpdateInfo(true, new TimeSpan(20));
            _serverData.UpdateInfo(true, new TimeSpan(10));
            var actualTimeSpawn = _serverData.AverageTimeResponse;

            expectedTimeSpan.Should().Be(actualTimeSpawn);
        }

        [Test]
        public void UpdateInfo_CorrectUpdateFailedTimesWhenNoSuccess()
        {
            var expectedTimes = 1;
            _serverData.UpdateInfo(false);
            var actualTimes = _serverData.FailedTimes;

            expectedTimes.Should().Be(actualTimes);
        }

        [Test]
        public void UpdateInfo_NoUpdateAverageTimeWhenNoSuccess()
        {
            var expectedTimeSpan = new TimeSpan(0);
            
            _serverData.UpdateInfo(false);
            var actualTimeSpawn = _serverData.AverageTimeResponse;

            expectedTimeSpan.Should().Be(actualTimeSpawn);
        }
        
        [Test]
        public void UpdateInfo_NoUpdateFailedTimesWhenSuccess()
        {
            var expectedTimes = 0;
            _serverData.UpdateInfo(true, TimeSpan.Zero);
            var actualTimes = _serverData.FailedTimes;

            expectedTimes.Should().Be(actualTimes);
        }
    }
}