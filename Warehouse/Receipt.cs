using System;
using System.Collections.Generic;

namespace Warehouse
{
	public class Receipt
	{
		public Guid UUID { get; set; }
		public string Key { get; set; }
		public IList<LoadingDockPolicy> Policies { get; set; }
	}
}