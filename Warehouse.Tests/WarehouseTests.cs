using System;
using Xunit;

namespace Warehouse.Tests
{
    public class WarehouseTests
    {
        [Fact]
        public void MemoryWarehouseShouldStoreAndReturnPallet()
        {
			var warehouse = new Warehouse();

			var payload = "Test Test test";
			var key = "item1";

			var receipt = warehouse.Store(key, payload, new[] { LoadingDockPolicy.Ephemeral });

		}
    }
}
