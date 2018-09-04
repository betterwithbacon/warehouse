using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	/// <summary>
	/// Stores a particular type and scope of data
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IShelf<T> : IShelf, IEqualityComparer<IShelf<T>>
	{	
		// Storage operations
		void Append(WarehouseKey key, IEnumerable<T> additionalPayload);
		void Store(WarehouseKey key, IEnumerable<T> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies);
		IEnumerable<T> Retrieve(WarehouseKey key);
	}

	public interface IShelf
	{
		void Initialize(IWarehouse warehouse, IStorageScope scope);

		// Identification aspects
		string Identifier { get; }
		IWarehouse Warehouse { get; }
		IStorageScope Scope { get; }

		// Retrieval Operations
		bool CanRetrieve(WarehouseKey key);
		ShelfManifest GetManifest(WarehouseKey key);

		// Discovery Operations
		bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
	}
}
