using BusDriver.Core.Events;
using BusDriver.Core.Scheduling;
using Lighthouse.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseCore.Apps
{
	public class WarehouseServer : LighthouseServiceBase, IWarehouse<string, string>
	{
		private readonly Warehouse warehouse = new Warehouse(initImmediately: false);
		private readonly ConcurrentBag<WarehouseServer> RemoteWarehouseServers = new ConcurrentBag<WarehouseServer>();

		public IEnumerable<IWarehouse<string, string>> AllWarehouses => RemoteWarehouseServers.SelectMany(ws => ws.AllWarehouses).Concat(new[] { warehouse });

		protected override void OnStart()
		{
			// start a local warehouse
			warehouse.Initialize();
		}

		protected override void OnAfterStart()
		{
			// schedule some work to be done
			LighthouseContainer.EventContext.AddScheduledAction(new Schedule { }, (time) => { PerformStorageMaintenance(time); });

			// populate the remote warehouses			
			LoadRemoteWarehouses();
		}

		private void LoadRemoteWarehouses()
		{
			// the container is how remote lighthouse resources are found
			if (LighthouseContainer != null)
			{
				LighthouseContainer.Log(Lighthouse.Core.Logging.LogLevel.Debug, this, "Loading remote warehouses.");

				// the Lighthouse context should know about the other services that are running
				foreach (var remoteWarehouseServer in LighthouseContainer.FindServices<WarehouseServer>())
				{
					// skip THIS service.
					if (remoteWarehouseServer.Id == this.Id)
						continue;

					RemoteWarehouseServers.Add(remoteWarehouseServer);
					LighthouseContainer.Log(Lighthouse.Core.Logging.LogLevel.Debug, this, $"Container local warehouse {remoteWarehouseServer} was added.");
				}

				// this is where an network discovery will occur. to reach other points, not local to this lighthouse runtime.
				// currently, this isn't implemented, but ideally
				foreach (var remoteWarehouseServer in LighthouseContainer.FindRemoteServices<WarehouseServer>())
				{
					// skip THIS service.
					if (remoteWarehouseServer.Id == this.Id)
						continue;

					RemoteWarehouseServers.Add(remoteWarehouseServer);
					LighthouseContainer.Log(Lighthouse.Core.Logging.LogLevel.Debug, this, $"Remote warehouse {remoteWarehouseServer} was added.");
				}
			}
		}

		public IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> policies)
		{
			return AllWarehouses.SelectMany(war => war.ResolveShelves(policies));
		}

		public IEnumerable<IShelf> ResolveRemoteShelves(IEnumerable<LoadingDockPolicy> policies)
		{
			return AllWarehouses.SelectMany(war => war.ResolveShelves(policies));
		}

		private void PerformStorageMaintenance(DateTime date)
		{
		}

		void IWarehouse<string, string>.Initialize()
		{
			throw new NotImplementedException("This warehouse server should be initialized within a lighthouse context.");
		}

		public Receipt Store(string key, IStorageScope scope, IList<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			var receipt = warehouse.Store(key, scope, data, loadingDockPolicies);
			RaiseStatusUpdated($"Data stored: {key}. Data Checksum: {receipt.SHA256Checksum}");

			return receipt;
		}

		public void Append(string key, IStorageScope scope, IEnumerable<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			warehouse.Append(key, scope, data, loadingDockPolicies);
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			return warehouse.Retrieve(key, scope);
		}

		public WarehouseKeyManifest GetManifest(string key, IStorageScope scope)
		{
			return warehouse.GetManifest(key, scope);
		}
	}
}
