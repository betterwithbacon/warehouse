using System.Collections.Generic;

namespace WarehouseCore
{
	public interface IStorageScope : IEqualityComparer<IStorageScope>
	{
		string ScopeName { get; }

		string Identifier { get; }
	}
}