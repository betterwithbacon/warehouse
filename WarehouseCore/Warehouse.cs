using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseCore
{
	public class Warehouse : IWarehouse<string,string>
	{
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IShelf<string, string>> Shelves = new ConcurrentBag<IShelf<string, string>>();

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

			// Discover shelf types
			foreach (var shelf in DiscoverShelves())
			{
				Shelves.Add(shelf);
				shelf.Initialize(this);
			}
		}

		public IEnumerable<IShelf<string,string>> DiscoverShelves()
		{
			yield return new MemoryShelf();

			foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(IShelf<string, string>).IsAssignableFrom(t) && t.IsClass))
			{
				yield return Activator.CreateInstance(type) as IShelf<string, string>;
			}
		}

		public Receipt Store(string key, IStorageScope scope, IList<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();

			var uuid = Guid.NewGuid();

			ConcurrentBag<LoadingDockPolicy> enforcedPolicies = new ConcurrentBag<LoadingDockPolicy>();

			// resolve the appropriate store, based on the policy
			Parallel.ForEach(ResolveShelves(loadingDockPolicies), (shelf) =>
			{
				shelf.Store(key, scope, data, enforcedPolicies);
			});

			// the receipt is largely what was passed in when it was stored
			var receipt = new Receipt(enforcedPolicies.Any())
			{
				UUID = uuid,
				Key = key,
				Scope = scope,
				// add the policies that were upheld during the store, this is necessary, 
				// because this warehouse might not be able to satisfy all of the policies
				Policies = enforcedPolicies.Distinct().ToList(),
				SHA256Checksum = CalculateChecksum(data)				
			};

			SessionReceipts.Add(receipt);

			return receipt;
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

		public IEnumerable<IShelf<string, string>> ResolveShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return Shelves.Where(s => s.CanEnforcePolicies(loadingDockPolicies));
		}

		//public IEnumerable<IShelf> ResolveLocalShelves(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		//{
		//	return ResolveShelves(loadingDockPolicies);
		//}

		void ThrowIfNotInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Warehouse not initialized.");
		}

		public static string CalculateChecksum(IList<string> input)
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

		public static bool VerifyChecksum(IList<string> input, string hash)
		{
			return CalculateChecksum(input).Equals(hash);
		}

		public WarehouseKeyManifest GetManifest(string key, IStorageScope scope)
		{
			// right now, we just return the data that was sent when it was created
			var policies = SessionReceipts.FirstOrDefault(sr => sr.Key == key)?.Policies;

			// if there aren't any receipts for this, the warehouse has no idea where they're stored. 
			// TODO: ideally, the warehouse will eventually be able to resolve the receipts from their state
			if (policies == null)
				return new WarehouseKeyManifest();
				
			return new WarehouseKeyManifest
			{
				StorageShelvesManifests = ResolveShelves(policies).Where(s => s.CanRetrieve(key, scope)).Select( shelf => shelf.GetManifest(key, scope)).ToList(),
				StoragePolicies = policies
			};	
		}
	}
}
