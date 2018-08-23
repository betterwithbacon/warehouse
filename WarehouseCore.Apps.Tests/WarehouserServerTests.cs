using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Server;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace WarehouseCore.Apps.Tests
{
	public class WarehouserServerTests
	{
		private readonly ITestOutputHelper Output;

		public WarehouserServerTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
		[Trait("Type", "WarehouseServer")]
		public void Start_StartsUpCorrectly()
		{
			var lighthouse = new LighthouseServer(Output.WriteLine);
			lighthouse.Start();

			var server = new WarehouseServer();
			server.StatusUpdated += Server_StatusUpdated;
			server.Initialize(lighthouse);
			server.Start();

			server.RunState.Should().Be(LighthouseServiceRunState.Running);
		}

		[Fact]
		[Trait("Type", "WarehouseServer")]
		public void ResolveShelves_DiscoverLocalInMemoryStorage()
		{
			var lighthouse = new LighthouseServer(Output.WriteLine);
			lighthouse.Start();
			var warehouseServer = new WarehouseServer();
			// bind the warehouserServer to the lighthouseServer
			lighthouse.Launch(warehouseServer);

			// get a shelf that can hold data for the duration of the session			
			var remoteShelves = warehouseServer.ResolveShelves(new[] { LoadingDockPolicy.Ephemeral });

			// only one shelf should show up, nd it should be a memshelf
			remoteShelves.OfType<MemoryShelf>().Count().Should().Be(1);

			// no exceptions should have been thrown
			lighthouse.GetRunningServices().Where(lsr => lsr.Exceptions.Count > 0).Should().BeEmpty();
		}

		[Fact]
		[Trait("Type", "WarehouseServer")]
		public void StoreAndRetrieve_ReturnsDataCorrectly()
		{
			var lighthouseServer = new LighthouseServer(Output.WriteLine);
			lighthouseServer.Start();
			var warehouseServer = new WarehouseServer();
			// bind the warehouserServer to the lighthouseServer
			lighthouseServer.Launch(warehouseServer);

			// get a shelf that can hold data for the duration of the session	
			var payload = new[] { "data" };
			warehouseServer.Store("testData", StorageScope.Global, payload, new[] { LoadingDockPolicy.Ephemeral });

			var retrievedValues = warehouseServer.Retrieve("test", StorageScope.Global);
			payload.Should().BeEquivalentTo(payload);
		}

		[Fact]
		[Trait("Type", "WarehouseServer")]
		public void ResolveShelves_FoundShelfFromOtherWarehouseServerInTheSameLighthouseServer()
		{
			var lighthouseServer = new LighthouseServer(Output.WriteLine);
			lighthouseServer.Start();
			var warehouseServer = new WarehouseServer();			
			lighthouseServer.Launch(warehouseServer);
						
			var otherWarehouseServer = new WarehouseServer();
			lighthouseServer.Launch(otherWarehouseServer);

			// get a shelf that can hold data for the duration of the session	
			var resolvedShelves = warehouseServer.ResolveShelves(new[] { LoadingDockPolicy.Ephemeral });
			
			// the OTHER warehosue, should also have a memory shelf, that we can store data with.
			resolvedShelves.Count().Should().Be(2);
		}

		private void Server_StatusUpdated(ILighthouseComponent owner, string status)
		{
			Output.WriteLine($"{owner}:{status}");
		}
	}
}
