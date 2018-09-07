﻿using Lighthouse.Core.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WarehouseCore.Disk
{
	public class LocalDiskShelf : IShelf<string>
	{
		public static readonly List<LoadingDockPolicy> SupportedPolicies = new List<LoadingDockPolicy> { LoadingDockPolicy.Persistent };
		public string Identifier => Guid.NewGuid().ToString();
		public IWarehouse Warehouse { get; private set; }
		public IStorageScope Scope { get; private set; }
		private readonly Dictionary<WarehouseKey, string> FileNames;
		public IFileSystemProvider FileSystemProvider { get; private set; }

		public LocalDiskShelf()
		{
			FileNames = new Dictionary<WarehouseKey, string>();			
		}

		public void Append(WarehouseKey key, IEnumerable<string> additionalPayload)
		{
			throw new NotImplementedException();
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return loadingDockPolicies.Intersect(SupportedPolicies).Any();
		}

		public bool CanRetrieve(WarehouseKey key)
		{
			var file = GetFilePath(key);
			return FileSystemProvider.FileExists(file);
		}

		public bool Equals(IShelf<string> x, IShelf<string> y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IShelf<string> obj)
		{
			return obj?.Identifier.GetHashCode() ?? -1;
		}

		public ShelfManifest GetManifest(WarehouseKey key)
		{
			// we can't support returning this data for real yet. It'd be good to pull this data from the file system			
			var manifest = new ShelfManifest(new[] { LoadingDockPolicy.Persistent }, -1);

			// TODO: pull some metadata from File system
			return manifest;
		}

		public void Initialize(IWarehouse warehouse, IStorageScope scope)
		{
			Warehouse = warehouse;
			Scope = scope;

			// TODO: if there are 3 file system providers, who do you know which one to use. 
			FileSystemProvider = Warehouse.LighthouseContainer.GetFileSystemProviders().FirstOrDefault();
			if (FileSystemProvider == null)
				throw new ApplicationException("No filesystem could be located.");
		}

		public IEnumerable<string> Retrieve(WarehouseKey key)
		{
			// TODO: don't do this asynchronously YET. I'm delaying converting everything to async, but not yet.
			var rawData = FileSystemProvider.ReadFromFileSystem(GetFilePath(key)).Result;

			if (rawData == null || rawData.Length == 0)
			{
				// no data, just return an empty file
				// TODO: should this be an exception? if there isn't any data. Or is it possible an empty file was created
				yield return "";
			}
			else
			{
				yield return Encoding.UTF8.GetString(rawData);
			}
		}

		public void Store(WarehouseKey key, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies)
		{
			var convertedPayload = new List<byte>();
			foreach(var rec in payload)
			{
				convertedPayload.AddRange(Encoding.UTF8.GetBytes(rec));
			}

			SupportedPolicies.ForEach((ldp) => enforcedPolicies.TryAdd(ldp));

			FileSystemProvider.WriteToFileSystem($"\\{key.Scope.Identifier}\\{key.Id}", convertedPayload.ToArray());
		}

		string GetFilePath(WarehouseKey key)
		{
			return $"\\{key.Scope.Identifier}\\{key.Id}";
		}
	}
}
