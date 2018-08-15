using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WarehouseCore.Tests
{
	public class WarehouseTests
	{
		private readonly ITestOutputHelper output;

		public WarehouseTests(ITestOutputHelper output)
		{
			this.output = output;
		}

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
		public void PayloadSigningShouldRoundtrip()
		{
			var warehouse = new Warehouse();
			warehouse.Initialize();

			var payload = new[] { "Test Test test" };
			var key = "item1";
			var scope = new ApplicationScope("TestApp");

			var receipt = warehouse.Store(key, scope, payload, new[] { LoadingDockPolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve(key, scope).ToList();

			warehouse.Verify(returnedValue, receipt.SHA256Checksum).Should().BeTrue();

			//receipt.SHA256Checksum.Should().Be( )
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

			warehouse.Append(key, scope, new[] { nextText }, receipt.Policies);

			var newReturnedValue = warehouse.Retrieve(key, scope);
			newReturnedValue.Should().Contain(payload);
		}

		[Fact]
		public void MultiThreadedWriteReadPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions {  MaxDegreeOfParallelism = 10 },
				(index) => {
					var payload = new[] { index.ToString() };
					warehouse.Store(index.ToString(), appScope, payload, new[] { LoadingDockPolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					warehouse.Retrieve(index.ToString(), appScope).Should().Contain(payload);
			});
		}

		[Fact]
		public void MultiThreadedAppendReadPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					var payload = new[] { index.ToString() };
					var additionalPayload = new[] { (index + 100).ToString() };
					warehouse.Store(index.ToString(), appScope, payload, new[] { LoadingDockPolicy.Ephemeral });
					warehouse.Append(index.ToString(), appScope, additionalPayload, new[] { LoadingDockPolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					warehouse.Retrieve(index.ToString(), appScope).Should().Contain(payload.Concat(additionalPayload));
				});
		}

		[Fact]
		public void MultiThreadedAppendReadToOneKeyPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			var key = "key";
			var payload = new[] { "initial" };
			warehouse.Store(key, appScope, payload, new[] { LoadingDockPolicy.Ephemeral });

			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					warehouse.Append(key, appScope, new[] { (index + 100).ToString() }, new[] { LoadingDockPolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					
				});

			warehouse.Retrieve(key, appScope).Count().Should().Be(101);
		}
	}
}
