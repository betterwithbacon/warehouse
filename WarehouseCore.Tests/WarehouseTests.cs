using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace WarehouseCore.Tests
{
	public class WarehouseTests
    {
        [Fact]
        public void MemoryWarehouseShouldStoreAndReturnPallet()
        {
			var warehouse = new Warehouse();
			warehouse.Initialize();

			var payload = new[] { "Test Test test" };
			var key = "item1";
			var scope = new ApplicationScope("TestApp");

			var policy = new LoadingDockPolicy();
			var receipt = warehouse.Store(key, scope, payload, new[] { LoadingDockPolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve(key, scope).ToList();

			returnedValue.Should().Contain(payload);
		}

		[Fact]
		public void MemoryWarehouseShouldStoreAndReturnAndAppendAndReturnPallet()
		{
			var warehouse = new Warehouse();
			warehouse.Initialize();

			var payload = new List<string>() { "Test Test test" };
			var key = "item1";
			var scope = new ApplicationScope("TestApp");

			var policy = new LoadingDockPolicy();
			var receipt = warehouse.Store(key, scope, payload, new[] { LoadingDockPolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve(key, scope).ToList();

			returnedValue.Should().Contain(payload);

			var nextText = " 123456789";
			payload.Add(nextText);

			warehouse.Append(key, scope, new[]{ nextText }, receipt.Policies);

			var newReturnedValue = warehouse.Retrieve(key, scope);
			newReturnedValue.Should().Contain(payload);
		}
	}
}
