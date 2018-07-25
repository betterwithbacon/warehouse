using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarehouseCore
{
	public class MemoryShelf : IShelf
	{
		private static readonly ConcurrentDictionary<MemoryShelfKey, List<string>> Records = new ConcurrentDictionary<MemoryShelfKey, List<string>>();
		public static readonly IList<LoadingDockPolicy> SupportedPolicies = new[] {  LoadingDockPolicy.Ephemeral };

		public bool CanRetrieve(string key, IStorageScope scope)
		{
			return Records[new MemoryShelfKey(scope, key)] != null;
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			return Records[new MemoryShelfKey(scope, key)];
		}

		public void Store(Guid uuid, string key, IStorageScope scope, IEnumerable<string> payload)
		{
			Records[new MemoryShelfKey(scope,key)] = payload.ToList();
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return SupportedPolicies.Intersect(loadingDockPolicies).Any();
		}

		struct MemoryShelfKey
		{
			public string ScopeIdentifier { get; }
			public string Key { get; }

			public MemoryShelfKey(IStorageScope scope, string key)
			{
				ScopeIdentifier = scope.Identifier;
				Key = key;
			}
		}
	}
}
