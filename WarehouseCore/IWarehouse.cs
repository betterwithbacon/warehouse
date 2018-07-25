using System;

namespace WarehouseCore
{
    public interface IWarehouse
    {
		bool IsInitialized { get; }
		void Initialize();
    }
}
