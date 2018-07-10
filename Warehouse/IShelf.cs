using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse
{
	public interface IShelf
	{
		void Store(Guid uuid, string key, string payload);
	}
}
