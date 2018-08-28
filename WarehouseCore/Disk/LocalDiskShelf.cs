using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WarehouseCore.Disk
{
	public class LocalDiskShelf : IShelf<string, string>
	{
		public static readonly IList<LoadingDockPolicy> SupportedPolicies = new[] { LoadingDockPolicy.Persistent };

		public string Identifier => Guid.NewGuid().ToString();

		public void Append(string key, IStorageScope scope, IEnumerable<string> additionalPayload)
		{
			throw new NotImplementedException();
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			throw new NotImplementedException();
		}

		public bool CanRetrieve(string key, IStorageScope scope)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IShelf<string, string> x, IShelf<string, string> y)
		{
			throw new NotImplementedException();
		}

		public int GetHashCode(IShelf<string, string> obj)
		{
			throw new NotImplementedException();
		}

		public ShelfManifest GetManifest(string key, IStorageScope scope)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IWarehouse<string, string> warehouse)
		{
			// check the local filesystem and see if a "warehouse" location exists			
			//Directory.Exists()
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			throw new NotImplementedException();
		}

		public void Store(string key, IStorageScope scope, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies)
		{
			throw new NotImplementedException();
		}
	}
}
