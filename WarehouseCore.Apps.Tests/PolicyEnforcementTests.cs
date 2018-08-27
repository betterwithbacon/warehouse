using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;


namespace WarehouseCore.Apps.Tests
{
    public class PolicyEnforcementTests
    {
		private readonly ITestOutputHelper Output;

		public PolicyEnforcementTests(ITestOutputHelper output)
		{
			Output = output;
		}

		WarehouseTestEnvironment GetWarehouseTestEnvironment() => new WarehouseTestEnvironment(Output.WriteLine);

		#region Ephemeral
		[Fact]
		[Trait(TestTraits.Type, "WarehouseServer")]
		[Trait(TestTraits.Function, "Store")]
		[Trait(TestTraits.Tag, TestTraits.Tags.WarehouseServer)]
		public void Ephemeral_ItemStoredWarehouseServer()
		{
			var testPayload = "testPayload";
			var testPayloadKey = "key";

			GetWarehouseTestEnvironment()
				.AddLighthouseRuntime()
				.AddWarehouseServer()
				.Store(testPayloadKey, testPayload, new[] { LoadingDockPolicy.Ephemeral })
				.AssertStored(testPayloadKey, testPayload)
				.AssertStoragePolicy(testPayloadKey);
		}
		#endregion

		#region Persistent
		[Fact]
		[Trait(TestTraits.Type, "WarehouseServer")]
		[Trait(TestTraits.Function, "Store")]
		[Trait(TestTraits.Tag, TestTraits.Tags.WarehouseServer)]
		public void Persistent_ItemStoredOnWarehouseServer()
		{
			var testPayload = "testPayload";
			var testPayloadKey = "key";

			GetWarehouseTestEnvironment()
				.AddLighthouseRuntime()
				.AddWarehouseServer()
				.Store(testPayloadKey, testPayload, new[] { LoadingDockPolicy.Persistent })
				.AssertStored(testPayloadKey, testPayload)
				.AssertStoragePolicy(testPayloadKey);
		}
		#endregion

		#region Archival
		[Fact]
		[Trait(TestTraits.Type, "WarehouseServer")]
		[Trait(TestTraits.Function, "Store")]
		[Trait(TestTraits.Tag, TestTraits.Tags.WarehouseServer)]
		public void Archival_ItemStoredOnWarehouseServer()
		{
			var testPayload = "testPayload";
			var testPayloadKey = "key";

			GetWarehouseTestEnvironment()
				.AddLighthouseRuntime()
				.AddWarehouseServer()
				.Store(testPayloadKey, testPayload, new[] { LoadingDockPolicy.Archival })
				.AssertStored(testPayloadKey, testPayload)
				.AssertStoragePolicy(testPayloadKey);
		}
		#endregion

		#region ColdStorage
		[Fact]
		[Trait(TestTraits.Type, "WarehouseServer")]
		[Trait(TestTraits.Function, "Store")]
		[Trait(TestTraits.Tag, TestTraits.Tags.WarehouseServer)]
		public void ColdStorage_ItemStoredOnWarehouseServer()
		{
			var testPayload = "testPayload";
			var testPayloadKey = "key";

			GetWarehouseTestEnvironment()
				.AddLighthouseRuntime()
				.AddWarehouseServer()
				.Store(testPayloadKey, testPayload, new[] { LoadingDockPolicy.ColdStorage })
				.AssertStored(testPayloadKey, testPayload)
				.AssertStoragePolicy(testPayloadKey);
		}
		#endregion
	}
}
