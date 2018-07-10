using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse
{
    public class LoadingDockPolicy
    {
		public static LoadingDockPolicy Ephemeral { get; }

		static LoadingDockPolicy()
		{
			Ephemeral = new LoadingDockPolicy()
			{
				
			};
		}
	}
}
