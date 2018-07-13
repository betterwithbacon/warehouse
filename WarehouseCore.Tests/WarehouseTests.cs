using FluentAssertions;
//using WarehouseCore;
using Xunit;

namespace WarehouseCore.Tests
{
	public class WarehouseTests
    {
        [Fact]
        public void MemoryWarehouseShouldStoreAndReturnPallet()
        {
			var warehouse = new Warehouse();

			var payload = "Test Test test";
			var key = "item1";

			var policy = new LoadingDockPolicy();
			var receipt = warehouse.Store(key, payload, new[] { LoadingDockPolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve(key);

			returnedValue.Should().Be(payload);


		}
    }
}
