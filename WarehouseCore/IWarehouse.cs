using Lighthouse.Core;
using System;
using System.Collections.Generic;

namespace WarehouseCore
{
	public interface IWarehouse : ILighthouseComponent // : IDictionary<WarehouseKey, TData><-- one day!
	{
		/// <summary>
		/// Places the warehouse in a state where it can store and retrieve data
		/// </summary>
		void Initialize();

		/// <summary>
		/// Stores the provided data, with the relevant key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		/// <returns></returns>
		Receipt Store<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies);

		/// <summary>
		/// Appends data to the existing data 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		void Append<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies);

		/// <summary>
		/// Returns the Data for a given key and scope
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		IEnumerable<T> Retrieve<T>(WarehouseKey key);

		/// <summary>
		/// Returns all of the available metadata for a given WarehouseKey.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		WarehouseKeyManifest GetManifest(WarehouseKey key);
	}
}
