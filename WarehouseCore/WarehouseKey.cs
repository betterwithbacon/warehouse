using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public sealed class WarehouseKey : IEqualityComparer<WarehouseKey>
	{
		public readonly string Id;
		public readonly IStorageScope Scope;
		public readonly Dictionary<string, object> OtherIdentifiers;

		private WarehouseKey() {
			Id = "";
			Scope = null;
			OtherIdentifiers = null;
		}

		public WarehouseKey(string uuid, IStorageScope scope, Dictionary<string, object> otherIdentifiers = null)
		{
			Id = uuid;
			Scope = scope;
			OtherIdentifiers = otherIdentifiers;
		}

		public override string ToString()
		{
			return $"[{Scope}]{Id}";
		}

		public bool Equals(WarehouseKey x, WarehouseKey y)
		{
			return x.Id == y.Id && x.Scope == y.Scope;
		}

		public int GetHashCode(WarehouseKey obj)
		{
			return Id.GetHashCode();
		}
	}
}
