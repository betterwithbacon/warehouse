using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public interface IShelf : IEqualityComparer<IShelf>
	{
		string Identifier { get; }
		void Append(string key, IStorageScope scope, IEnumerable<string> additionalPayload);
		void Store(string key, IStorageScope scope, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies);
		IEnumerable<string> Retrieve(string key, IStorageScope scope);

		bool CanRetrieve(string key, IStorageScope scope);
		bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
		
		ShelfManifest GetManifest(string key, IStorageScope scope);
	}
}
