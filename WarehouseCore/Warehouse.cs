﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lighthouse.Core;

namespace WarehouseCore
{
	public class Warehouse : IWarehouse
	{
		// TODO: this is atemp solution to this problem of discoverying available shelf types
		// ideally, this will be discovered by reflection
		public static ConcurrentBag<Type> AvailableShelfTypes;
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IShelf> Shelves = new ConcurrentBag<IShelf>();
		public event StatusUpdatedEventHandler StatusUpdated;

		private bool IsInitialized => Shelves.Count > 0;

		public ILighthouseServiceContainer LighthouseContainer { get; private set; }

		static Warehouse()
		{
			AvailableShelfTypes = new ConcurrentBag<Type>();
		}

		public static void RegisterShelfType(Type shelfType)
		{
			if (shelfType.IsAssignableFrom(typeof(IShelf)) && shelfType.IsClass)
				throw new ArgumentException("shelf type MUST be implement IShelf and be concrete.");

			if (!AvailableShelfTypes.Contains(shelfType))
				AvailableShelfTypes.Add(shelfType);
		}

		public Warehouse( bool initImmediately = true)
		{
			if(initImmediately)			
				Initialize(null);
		}

		public void Initialize(ILighthouseServiceContainer serviceContainer, params Type[] shelvesToUse)
		{
			// only do this once
			if (IsInitialized)
				return;

			// register with the service container
			LighthouseContainer = serviceContainer;

			// Discover shelf types
			foreach (var shelf in DiscoverShelves())
			{
				Shelves.Add(shelf);

				// create all the shelves in the global scope
				shelf.Initialize(this, StorageScope.Global);
			}

			foreach (var shelfType in shelvesToUse)
			{
				if (Activator.CreateInstance(shelfType) is IShelf shelf)
				{
					Shelves.Add(shelf);

					// create all the shelves in the global scope
					shelf.Initialize(this, StorageScope.Global);
				}
			}
		}

		public IEnumerable<IShelf> DiscoverShelves()
		{
			yield return new MemoryShelf();


			//foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) //.SelectMany(a => a.GetTypes()).Where(t => typeof(IShelf).IsAssignableFrom(t) && t.IsClass))
			//{
			//	foreach(var type in assembly.GetTypes())
			//	{
			//		bool isCorrect = false;

			//		try
			//		{
			//			if (typeof(IShelf).IsAssignableFrom(type) && type.IsClass)
			//			{
			//				isCorrect = true;
			//			}
			//		}
			//		catch (Exception) { }

			//		if(isCorrect)
			//			yield return Activator.CreateInstance(type) as IShelf;
			//	}
			//	//yield return Activator.CreateInstance(type) as IShelf;
			//}
		}

		public Receipt Store<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();

			var uuid = Guid.NewGuid();

			ConcurrentBag<LoadingDockPolicy> enforcedPolicies = new ConcurrentBag<LoadingDockPolicy>();

			// resolve the appropriate store, based on the policy
			Parallel.ForEach(ResolveShelves<T>(loadingDockPolicies), (shelf) =>
			{
				shelf.Store(key,data, enforcedPolicies);
			});

			// the receipt is largely what was passed in when it was stored
			var receipt = new Receipt(enforcedPolicies.Any())
			{
				UUID = uuid,
				Key = key.Id,
				Scope = key.Scope,
				// add the policies that were upheld during the store, this is necessary, 
				// because this warehouse might not be able to satisfy all of the policies				
				Policies = enforcedPolicies.Distinct().ToList(),
				SHA256Checksum = CalculateChecksum<T>(data)				
			};

			SessionReceipts.Add(receipt);

			return receipt;
		}

		public void Append<T>(WarehouseKey key, IEnumerable<T> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			ThrowIfNotInitialized();
			
			Parallel.ForEach(ResolveShelves<T>(loadingDockPolicies), (shelf) =>
			{
				shelf.Append(key, data);
			});
		}

		public IEnumerable<T> Retrieve<T>(WarehouseKey key)
		{
			ThrowIfNotInitialized();
			return 
				Shelves				
					// We can't just use the OfType, because we ned to handle inheritance
					// buy we do want this for speed.
					.OfType<IShelf<T>>()
					.FirstOrDefault(shelf => shelf.CanRetrieve(key))					
					.Retrieve(key) 
				?? Enumerable.Empty<T>();
		}

		public IEnumerable<IShelf<T>> ResolveShelves<T>(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return Shelves.OfType<IShelf<T>>().Where(s => s.CanEnforcePolicies(loadingDockPolicies));
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

		public static string CalculateChecksum<T>(IEnumerable<T> input)
		{
			// can't calculate checksums for non strings right now
			// TODO: add support for non-strings
			if (typeof(T) != typeof(String))
				return String.Empty;

			using (var sha256 = SHA256.Create())
			{
				byte[] data = sha256.ComputeHash(input.SelectMany(s => Encoding.UTF8.GetBytes(s as String)).ToArray());
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

		public WarehouseKeyManifest GetManifest(WarehouseKey key)
		{
			// right now, we just return the data that was sent when it was created
			var policies = SessionReceipts.FirstOrDefault(sr => sr.Key == key.Id)?.Policies;

			// if there aren't any receipts for this, the warehouse has no idea where they're stored. 
			// TODO: ideally, the warehouse will eventually be able to resolve the receipts from their state
			if (policies == null)
				return new WarehouseKeyManifest();
				
			return new WarehouseKeyManifest
			{
				// TODO: where do we get the type from? is it passed in? Why should it matter here?
				StorageShelvesManifests = ResolveShelves<object>(policies).Where(s => s.CanRetrieve(key)).Select( shelf => shelf.GetManifest(key)).ToList(),
				StoragePolicies = policies
			};	
		}
	}
}
