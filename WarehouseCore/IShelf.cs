using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public interface IShelf
	{
		void Store(Guid uuid, string key, IStorageScope scope, IEnumerable<string> payload);
		IEnumerable<string> Retrieve(string key, IStorageScope scope);
		bool CanRetrieve(string key, IStorageScope scope);
		bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies);
	}
}
