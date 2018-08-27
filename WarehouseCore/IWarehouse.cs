using System;
using System.Collections.Generic;

namespace WarehouseCore
{
    public interface IWarehouse<TKey, TData> // : IDictionary<TKey, TData><-- one day!
    {
		void Initialize();
		IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
		Receipt Store(TKey key, IStorageScope scope, IList<TData> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies);		
		void Append(TKey key, IStorageScope scope, IEnumerable<TData> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies);
		IEnumerable<TData> Retrieve(TKey key, IStorageScope scope);
		WarehouseKeyManifest GetManifest(TKey key, IStorageScope scope);
	}
}
