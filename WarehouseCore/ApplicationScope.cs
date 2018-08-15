using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public class ApplicationScope : IStorageScope
	{	
		public string ScopeName { get; }

		public string Identifier => ScopeName.ToLower().Replace(" ","_");

		public ApplicationScope(string name)
		{
			ScopeName = name;
		}
	}

	public sealed class StorageScope : IStorageScope
	{
		public static IStorageScope Global = new StorageScope("global");
		
		public string ScopeName { get; }

		public string Identifier => ScopeName;

		public StorageScope(string name)
		{
			ScopeName = name;
		}
	}

	public sealed class LocalScope : IStorageScope
	{	
		public string ScopeName { get; }

		public string Identifier => ScopeName;

		public LocalScope()
		{
			// generate a GUID to identify this scope. it's not expected to be persistent
			ScopeName = $"local_{Guid.NewGuid().ToString().Replace("-","")}";
		}
	}
}
