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
		static readonly ConcurrentBag<IShelf> Shelves = new ConcurrentBag<IShelf>();

		public Receipt Store(string key, string payload, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			var uuid = Guid.NewGuid();

			// resolve the appropriate store, based on the policy
			foreach (var shelf in ResolveShelves(loadingDockPolicies))
			{
				shelf.Store(uuid, key, payload);
			}

			// the receipt is largely what was passed in when it was stored
			var receipt = new Receipt
			{
				UUID = uuid,
				Key = key,
				Policies = loadingDockPolicies.ToList()
			};

			SessionReceipts.Add(receipt);

			return receipt;
		}

		public string Retrieve(string key)
		{
			return Shelves.FirstOrDefault(shelf => shelf.CanRetrieve(key))?.Retrieve(key) ?? string.Empty;
		}

		private IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// any ephemeral call, should store the data in a memory pool
			if (loadingDockPolicies.Contains(LoadingDockPolicy.Ephemeral))
			{
				if (Shelves.TryPeek(out var shelf))
					yield return shelf;
				else
				{
					var newShelf = new MemoryShelf();
					Shelves.Add(newShelf);
					yield return newShelf;
				}
			}
		}
	}
}
