using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseCore
{
	public class Warehouse : IWarehouse<string,string>
	{
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IShelf> Shelves = new ConcurrentBag<IShelf>();

		private bool IsInitialized => Shelves.Count > 0;

		public Warehouse(bool initImmediately = true)
		{
			if(initImmediately)			
				Initialize();			
		}

		public void Initialize()
		{
			// only do this once
			if (IsInitialized)
				return;

			// pre-seed the shelves with all available storage types
			var newShelf = new MemoryShelf();
			Shelves.Add(newShelf);
		}

		public Receipt Store(string key, IStorageScope scope, IList<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();

			var uuid = Guid.NewGuid();

			// resolve the appropriate store, based on the policy
			Parallel.ForEach(ResolveShelves(loadingDockPolicies), (shelf) =>
			{
				shelf.Store(key, scope, data);
			});

			// the receipt is largely what was passed in when it was stored
			var receipt = new Receipt
			{
				UUID = uuid,
				Key = key,
				Scope = scope,
				Policies = loadingDockPolicies.ToList(),
				SHA256Checksum = GetSHA256Checksum(data)
			};

			SessionReceipts.Add(receipt);

			return receipt;
		}

		public bool Verify(List<string> returnedValue, string sHA256Checksum)
		{
			return true;	
		}

		public void Append(string key, IStorageScope scope, IEnumerable<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();
			
			Parallel.ForEach(ResolveShelves(loadingDockPolicies), (shelf) =>
			{
				shelf.Append(key, scope, data);
			});
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			ThrowIfNotInitialized();
			return Shelves.FirstOrDefault(shelf => shelf.CanRetrieve(key, scope))?.Retrieve(key, scope) ?? Enumerable.Empty<string>();
		}

		public IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return Shelves.Where(s => s.CanEnforcePolicies(loadingDockPolicies));
		}

		void ThrowIfNotInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Warehouse not initialized.");
		}

		static string GetSHA256Checksum(IList<string> input)
		{
			using (var sha256 = SHA256.Create())
			{
				byte[] data = sha256.ComputeHash(input.SelectMany(s => Encoding.UTF8.GetBytes(s)).ToArray());
				var sBuilder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)				
					sBuilder.Append(data[i].ToString("x2"));
				
				return sBuilder.ToString();
			}
		}

		static bool VerifySHA256Checksum(IList<string> input, string hash)
		{
			return GetSHA256Checksum(input).Equals(hash, StringComparison.OrdinalIgnoreCase);
		}
	}
}
