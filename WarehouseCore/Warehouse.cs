using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarehouseCore
{
	public class Warehouse : IWarehouse
	{
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IShelf> Shelves = new ConcurrentBag<IShelf>();

		public bool IsInitialized => Shelves.Count > 0;

		public Warehouse(bool initImmediately = true)
		{
			if(initImmediately)			
				Initialize();			
		}

		public void Initialize()
		{
			// only do this once
			if (IsInitialized)
				return;

			// pre-seed the shelves with all available storage types
			var newShelf = new MemoryShelf();
			Shelves.Add(newShelf);
		}

		public Receipt Store(string key, IStorageScope scope, IList<string> payload, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();

			var uuid = Guid.NewGuid();

			// resolve the appropriate store, based on the policy
			foreach (var shelf in ResolveShelves(loadingDockPolicies))
			{
				shelf.Store(uuid, key, scope, payload);
			}

			// the receipt is largely what was passed in when it was stored
			var receipt = new Receipt
			{
				UUID = uuid,
				Key = key,
				Scope = scope,
				Policies = loadingDockPolicies.ToList()
			};

			SessionReceipts.Add(receipt);

			return receipt;
		}

		public void Append(string key, IStorageScope scope, IEnumerable<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();
			var log = Retrieve(key, scope);
			Store(key, scope, log.Concat(data).ToList(), loadingDockPolicies);
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			ThrowIfNotInitialized();
			return Shelves.FirstOrDefault(shelf => shelf.CanRetrieve(key, scope))?.Retrieve(key, scope) ?? Enumerable.Empty<string>();
		}

		private IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return Shelves.Where(s => s.CanEnforcePolicies(loadingDockPolicies));
			
			//// any ephemeral call, should store the data in a memory pool
			//if (loadingDockPolicies.Contains(LoadingDockPolicy.Ephemeral))
			//{
			//	if (Shelves.TryPeek(out var shelf))
			//		yield return shelf;				
			//}
		}

		public void ThrowIfNotInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Warehouse not initialized.");
		}
	}
}
