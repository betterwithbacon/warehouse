using System.Collections.Generic;

namespace WarehouseCore
{
	/// <summary>
	/// Represents a simple manifest of a shelf. The manifest will be limited by Scope and, optionally, key
	/// </summary>
	public class ShelfManifest
	{
		/// <summary>
		/// The policies that are valid for either this
		/// </summary>
		public IList<LoadingDockPolicy> SupportedPolicies { get; private set; }

		/// <summary>
		/// Size of payload in Bytes
		/// </summary>
		public long StorageSize { get; private set; }

		public ShelfManifest(IList<LoadingDockPolicy> supportedPolicies, long storageSize)
		{
			SupportedPolicies = supportedPolicies;
			StorageSize = storageSize;
		}
	}
}