using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public class MemoryShelf : IShelf
	{
		private static readonly ConcurrentDictionary<string, string> Records = new ConcurrentDictionary<string, string>();

		public bool CanRetrieve(string key)
		{
			return Records[key] != null;
		}

		public string Retrieve(string key)
		{
			return Records[key];
		}

		public void Store(Guid uuid, string key, string payload)
		{
			Records.TryAdd(key, payload);
		}
	}
}
