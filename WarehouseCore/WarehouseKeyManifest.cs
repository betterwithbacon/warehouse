using System.Collections.Generic;

namespace WarehouseCore
{
	public class WarehouseKeyManifest
	{
		public IList<LoadingDockPolicy> StoragePolicies { get; internal set; }
		public IEnumerable<ShelfManifest> StorageShelvesManifests { get; internal set; }
	}
}