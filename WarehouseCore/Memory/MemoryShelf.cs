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
			return Records.Keys.Contains(new MemoryShelfKey(scope, key));
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			return Records.GetValueOrDefault(new MemoryShelfKey(scope, key), new List<string>());
		}

		public void Append(string key, IStorageScope scope, IEnumerable<string> additionalPayload)
		{
			Records.AddOrUpdate(new MemoryShelfKey(scope, key), additionalPayload.ToList(), (k, a) => a.Concat(additionalPayload).ToList());
		}

		public void Store(string key, IStorageScope scope, IEnumerable<string> payload)
		{
			Records.AddOrUpdate(new MemoryShelfKey(scope, key), payload.ToList(),(k,a) => payload.ToList());
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

			public override bool Equals(object obj)
			{
				if (obj is MemoryShelfKey msk)
					return msk.GetHashCode() == this.GetHashCode();
				else
					return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine<string, string>(ScopeIdentifier, Key);
			}
		}
	}
}
