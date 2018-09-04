using FluentAssertions;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WarehouseCore.Apps.Tests
{
    public class WarehouseTestEnvironment
    {
		private readonly Action<string> WriteLine;
		private readonly Action<string> Log;
		private readonly IStorageScope StorageScope = new LocalScope();
		public LighthouseServer LighthouseServer { get; private set; }
		public readonly List<WarehouseServer> WarehouseServers = new List<WarehouseServer>();
		private readonly Dictionary<string, Receipt> StorageReceipts = new Dictionary<string, Receipt>();

		public WarehouseTestEnvironment(Action<string> writeLine)
		{
			WriteLine = writeLine;
			Log = (string arg) => writeLine($"[{DateTime.Now.ToString("hh:mm:fff")}] [TEST ENV] {arg}");
		}

		public WarehouseTestEnvironment AddLighthouseRuntime()
		{
			if (LighthouseServer == null)
			{
				LighthouseServer = new LighthouseServer(WriteLine);				
				LighthouseServer.Start();
				Log($"Starting lighthouse server {LighthouseServer}.");
			}
			
			return this;
		}

		public WarehouseTestEnvironment AddWarehouseServer()
		{
			var warehouseServer = new WarehouseServer();
			WarehouseServers.Add(warehouseServer);

			Log($"[CONFIG] Adding warehouse server. {warehouseServer}");

			LighthouseServer?.Launch(warehouseServer);
			Thread.Sleep(100);
			return this;
		}

		public WarehouseTestEnvironment AssertStoragePolicy(string testPayloadKey, int warehouseIndex = 0)
		{
			var storeKey = new WarehouseKey(testPayloadKey, StorageScope);
			var receipt = StorageReceipts[testPayloadKey];

			if (receipt == null)
				throw new ApplicationException("No receipt was found for test payloadf");

			var manifest = WarehouseServers[warehouseIndex].GetManifest(storeKey);

			Log($"[ASSERTION] Policies. Found: {string.Join(',',manifest.StoragePolicies)}. Expected: {string.Join(',', receipt.Policies)}");
			
			// all of the policies being enforced should be in the manifest
			manifest.StoragePolicies.Should().Contain(receipt.Policies);

			return this;
		}

		public WarehouseTestEnvironment AssertStored(string testPayloadKey, string testPayload, int warehouseIndex = 0)
		{
			var storeKey = new WarehouseKey(testPayloadKey, StorageScope);
			var warehouse = WarehouseServers[warehouseIndex];
			var retrievedData = warehouse.Retrieve<string>(storeKey);
			retrievedData.Should().NotBeEmpty();

			// retrieve the receipt created when stored, and check the hashes (this is an overlap of other tests, but a bit of a sanity check)
			var receipt = StorageReceipts[testPayloadKey];

			Log($"[ASSERTION] Stored. Found: {retrievedData.First()}. Expected: {testPayload}");

			Warehouse.VerifyChecksum(new[] { testPayload }, receipt.SHA256Checksum);
			
			return this;
		}

		public WarehouseTestEnvironment Store(string key, string payload, LoadingDockPolicy[] loadingDockPolicies, int warehouseIndex = 0)
		{
			var warehouseKey = new WarehouseKey(key, scope: StorageScope);
			Log($"[Action] Storing. Key: {key}. Payload: {payload.Substring(0, LesserOfTwo(payload.Length,50))}");
			var storageReceipt = WarehouseServers[warehouseIndex].Store(warehouseKey, new[] { payload }, loadingDockPolicies);
			StorageReceipts.Add(key, storageReceipt);
			storageReceipt.WasSuccessful.Should().BeTrue();
			return this;
		}

		static int LesserOfTwo(int val1, int val2)
		{
			return val1 < val2 ? val1 : val2;
		}
	}
}
