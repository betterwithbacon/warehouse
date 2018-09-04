using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WarehouseCore.Disk
{
	public class LocalDiskShelf : IShelf<string>
	{
		public static readonly IList<LoadingDockPolicy> SupportedPolicies = new[] { LoadingDockPolicy.Persistent };
		public string Identifier => Guid.NewGuid().ToString();
		public IWarehouse Warehouse { get; private set; }
		public IStorageScope Scope { get; private set; }
		private readonly Dictionary<WarehouseKey, string> FileNames;

		public LocalDiskShelf()
		{
			FileNames = new Dictionary<WarehouseKey, string>();
		}

		public void Append(WarehouseKey key, IEnumerable<string> additionalPayload)
		{
			throw new NotImplementedException();
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return loadingDockPolicies.Intersect(SupportedPolicies).Any();
		}

		public bool CanRetrieve(WarehouseKey key)
		{
			return false;
		}

		public bool Equals(IShelf<string> x, IShelf<string> y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IShelf<string> obj)
		{
			return obj?.Identifier.GetHashCode() ?? -1;
		}

		public ShelfManifest GetManifest(WarehouseKey key)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IWarehouse warehouse, IStorageScope scope)
		{
			Warehouse = warehouse;
			Scope = scope;
		}

		public IEnumerable<string> Retrieve(WarehouseKey key)
		{
			throw new NotImplementedException();
		}

		public void Store(WarehouseKey key, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies)
		{
			throw new NotImplementedException();
		}
	}
}
