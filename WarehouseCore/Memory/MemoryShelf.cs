using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseCore
{
	public class MemoryShelf : IShelf<string,string>
	{
		private static readonly ConcurrentDictionary<MemoryShelfKey, List<string>> Records = new ConcurrentDictionary<MemoryShelfKey, List<string>>();
		public static readonly IList<LoadingDockPolicy> SupportedPolicies = new[] {  LoadingDockPolicy.Ephemeral };

		public string Identifier => Guid.NewGuid().ToString();

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

		public void Store(string key, IStorageScope scope, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies)
		{
			Records.AddOrUpdate(new MemoryShelfKey(scope, key), payload.ToList(),(k,a) => payload.ToList());
			foreach(var pol in SupportedPolicies)
				enforcedPolicies.TryAdd(pol);
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return SupportedPolicies.Any(
				sp => loadingDockPolicies.Any(
					ldp => ldp.IsEquivalentTo(sp)
				)
			);	
		}

		public bool Equals(IShelf<string,string> x, IShelf<string, string> y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IShelf<string, string> obj)
		{
			// TODO: this is TERRIBLE, because it means that another shelf with the same ID, could be confused for this one
			return obj.Identifier.GetHashCode();
		}

		public ShelfManifest GetManifest(string key, IStorageScope scope)
		{
			if(CanRetrieve(key, scope))
			{
				// the only way to get metadata from a memory shelf is to actually just retrieve it. 
				// TODO: store metadata separately for items as stord
				return new ShelfManifest(SupportedPolicies, CalculateSize(Retrieve(key, scope)));
			}

			return null;
		}

		private long CalculateSize(IEnumerable<string> records)
		{
			long size = records.Aggregate(0, 
				(totalRecordSize, record) => totalRecordSize += Encoding.Unicode.GetByteCount(record)
			);

			return size;
		}

		public void Initialize(IWarehouse<string, string> warehouse)
		{
			// There's no init work for a memshelf
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
