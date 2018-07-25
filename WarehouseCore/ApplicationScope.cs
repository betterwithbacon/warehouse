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
}
