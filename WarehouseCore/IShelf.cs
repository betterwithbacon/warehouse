using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public interface IShelf
	{
		void Append(string key, IStorageScope scope, IEnumerable<string> additionalPayload);
		void Store(string key, IStorageScope scope, IEnumerable<string> payload);
		IEnumerable<string> Retrieve(string key, IStorageScope scope);
		bool CanRetrieve(string key, IStorageScope scope);
		bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
	}
}
