using BusDriver.Core.Events;
using BusDriver.Core.Scheduling;
using Lighthouse.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WarehouseCore.Disk;

namespace WarehouseCore.Apps
{
	public class WarehouseServer : LighthouseServiceBase, IWarehouse
	{
		private readonly Warehouse warehouse = new Warehouse(initImmediately: false);
		private readonly ConcurrentBag<WarehouseServer> RemoteWarehouseServers = new ConcurrentBag<WarehouseServer>();

		public IEnumerable<Warehouse> AllWarehouses => RemoteWarehouseServers.SelectMany(ws => ws.AllWarehouses).Concat(new[] { warehouse });

		protected override void OnStart()
		{
			// start a local warehouse			
			warehouse.Initialize(
				LighthouseContainer,
				typeof(LocalDiskShelf)
			);
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

		//public IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> policies)
		//{
		//	return AllWarehouses.SelectMany(war => war.ResolveShelves(policies));
		//}

		private void PerformStorageMaintenance(DateTime date)
		{
		}

		void IWarehouse.Initialize(ILighthouseServiceContainer container, params Type[] shelvesToUse)
		{
			throw new NotImplementedException("This warehouse server should be initialized within a lighthouse context.");
		}

		public Receipt Store<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			var receipt = warehouse.Store(key, data, loadingDockPolicies);
			RaiseStatusUpdated($"Data stored: {key}. Data Checksum: {receipt.SHA256Checksum}");

			return receipt;
		}

		public void Append<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			warehouse.Append(key, data, loadingDockPolicies);
		}

		public IEnumerable<T> Retrieve<T>(WarehouseKey key)
		{
			return warehouse.Retrieve<T>(key);
		}

		public WarehouseKeyManifest GetManifest(WarehouseKey key)
		{
			return warehouse.GetManifest(key);
		}
	}
}
