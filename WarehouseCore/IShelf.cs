using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public interface IShelf<TKey,TData> : IEqualityComparer<IShelf<TKey, TData>>
	{
		void Initialize(IWarehouse<TKey, TData> warehouse);

		string Identifier { get; }
		void Append(TKey key, IStorageScope scope, IEnumerable<TData> additionalPayload);
		void Store(TKey key, IStorageScope scope, IEnumerable<TData> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies);
		IEnumerable<TData> Retrieve(TKey key, IStorageScope scope);

		bool CanRetrieve(string key, IStorageScope scope);
		bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
		
		ShelfManifest GetManifest(TKey key, IStorageScope scope);
	}
}
