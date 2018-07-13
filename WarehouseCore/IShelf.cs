using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseCore
{
	public interface IShelf
	{
		void Store(Guid uuid, string key, string payload);
		string Retrieve(string key);
		bool CanRetrieve(string key);
	}
}
