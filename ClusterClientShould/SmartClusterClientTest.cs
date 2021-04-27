using System;
using System.Diagnostics;
using System.Linq;
using ClusterClient;
using ClusterClient.Clients;
using ClusterClient.Clients.ArgumentParsers;
using FluentAssertions;
using log4net;
using NUnit.Framework;

namespace ClusterTests
{
	public class SmartClusterClientTest : ClusterTest
	{
		protected override SmartClusterClient CreateClient(string[] replicaAddresses)
			=> new SmartClusterClient(new ServersManager(new FCLArgumentParser(),replicaAddresses ),LogManager.GetLogger(typeof(SmartClusterClientTest)));

		[Test]
		public void ShouldReturnSuccessWhenLastReplicaIsGoodAndOthersAreSlow()
		{
			for (int i = 0; i < 3; i++)
				CreateServer(Slow);
			CreateServer(Fast);

			ProcessRequests(Timeout).Last().Should().BeCloseTo(TimeSpan.FromMilliseconds(3 * Timeout / 4 + Fast), TimeSpan.FromMilliseconds(Epsilon));
		}

		[Test]
		public void ShouldReturnSuccessWhenLastReplicaIsGoodAndOthersAreBad()
		{
			for (int i = 0; i < 3; i++)
				CreateServer(1, status: 500);
			CreateServer(Fast);

			ProcessRequests(Timeout).Last().Should().BeCloseTo(TimeSpan.FromMilliseconds(Fast), TimeSpan.FromMilliseconds(Epsilon));
		}

		[Test]
		public void ShouldThrowAfterTimeout()
		{
			for (var i = 0; i < 10; i++)
				CreateServer(Slow);

			var sw = Stopwatch.StartNew();
			Assert.Throws<TimeoutException>(() => ProcessRequests(Timeout));
			sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(Timeout), TimeSpan.FromMilliseconds(Epsilon));
		}

		[Test]
		public void ShouldNotForgetPreviousAttemptWhenStartNew()
		{
			CreateServer(4500);
			CreateServer(3000);
			CreateServer(10000);

			foreach(var time in ProcessRequests(6000))
				time.Should().BeCloseTo(TimeSpan.FromMilliseconds(4500), TimeSpan.FromMilliseconds(Epsilon));
		}

		[Test]
		public void ShouldNotSpendTimeOnBad()
		{
			CreateServer(1, status: 500);
			CreateServer(1, status: 500);
			CreateServer(4000);
			CreateServer(10000);

			foreach(var time in ProcessRequests(6000))
				time.Should().BeCloseTo(TimeSpan.FromMilliseconds(4000), TimeSpan.FromMilliseconds(Epsilon));
		}
	}
}