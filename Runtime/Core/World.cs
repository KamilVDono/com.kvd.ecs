using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core.Systems;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Core
{
	public class World
	{
		protected const char ControlCharacter = 'w';
		public readonly ComponentsStorage defaultStorage;

		protected readonly List<ISystem> systems = new();
		readonly Dictionary<ComponentsStorageKey, ComponentsStorage> _componentsStorages =
			new(4, ComponentsStorageKey.ComponentStorageKeyComparer);
		readonly List<ComponentsStorage> _componentsStoragesList = new(4);
		readonly IBootstrapable[] _bootstrapables;
		readonly Dictionary<Type, object> _singletons = new();

		bool _initialized;
		bool _inUpdatePhase;

		public bool IsRestored{ get; private set; }
		public IReadOnlyList<ComponentsStorage> AllComponentsStorages => _componentsStoragesList;

		public World(IBootstrapable[] bootstrapables)
		{
			_bootstrapables = bootstrapables;
			
			defaultStorage = RegisterComponentsStorage(ComponentsStorageKey.Default, new ComponentsStorage());
		}

		#region Lifetime
		public async UniTask Initialize()
		{
			if (_initialized)
			{
				throw new ApplicationException("World already initialized");
			}
			IsRestored = false;
			await Bootstrap();
			await InitInitialSystems();

			_initialized = true;
		}
		
		public async UniTask Restore(BinaryReader reader)
		{
			if (_initialized)
			{
				throw new ApplicationException("World already initialized");
			}
			//Deserialize(reader);
			IsRestored = true;
			await Bootstrap();
			await RestoreSystems();
			_initialized = true;
		}

		public async UniTask Destroy()
		{
			foreach (var system in systems)
			{
				await system.Destroy();
			}
			systems.Clear();
			
			_componentsStorages.Clear();
			foreach (var components in _componentsStoragesList)
			{
				components.Dispose();
			}
			_componentsStoragesList.Clear();

			_singletons.Clear();
		}

		public void Update()
		{
			_inUpdatePhase = true;
			var count = systems.Count;
			for (var i = 0; i < count; i++)
			{
				var system = systems[i];
				system.DoUpdate();
			}
			for (var i = 0; i < _componentsStoragesList.Count; i++)
			{
				_componentsStoragesList[i].ClearSingleFrameEntities();
			}
			#if STORAGES_CHECKS
			for (var i = 0; i < _componentsStoragesList.Count; i++)
			{
				_componentsStoragesList[i].SafetyCheck();
			}
			#endif
			_inUpdatePhase = false;
		}

		public void Save(BinaryWriter writer)
		{
			if (_inUpdatePhase)
			{
				Debug.LogError("Cannot save in update phase");
				return;
			}
			//Serialize(writer);
		}
		#endregion Lifetime

		#region Systems
		public async UniTask RegisterSystem(ISystem system)
		{
			systems.Add(system);
			system.Prepare();
			if (_initialized)
			{
				await system.Init(this);
			}
		}

		public async UniTask RemoveSystem(ISystem system)
		{
			systems.Remove(system);
			if (_initialized)
			{
				await system.Destroy();
			}
		}
		
		public T System<T>() where T : class, ISystem
		{
			for (var i = 0; i < systems.Count; ++i)
			{
				var system = FindSystem<T>(systems[i]);
				if (system != null)
				{
					return system;
				}
			}
			throw new ArgumentException($"There is no system of {typeof(T).Name}");
		}

		T? FindSystem<T>(ISystem system) where T : class, ISystem
		{
			{
				if (system is T targetSystem)
				{
					return targetSystem;
				}
			}

			for (var i = 0; i < system.InternalSystems.Count; i++)
			{
				var internalSystem = system.InternalSystems[i];
				var targetSystem   = FindSystem<T>(internalSystem);
				if (targetSystem != null)
				{
					return targetSystem;
				}
			}

			return null;
		}

		async UniTask InitInitialSystems()
		{
			foreach (var system in systems)
			{
				await system.Init(this);
			}
		}

		async UniTask RestoreSystems()
		{
			foreach (var system in systems)
			{
				await system.Restore(this);
			}
		}
		#endregion Systems

		#region ComponentsStorages
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Storage<T>(ComponentsStorageKey key) where T : ComponentsStorage
		{
			return (T)_componentsStorages[key];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentsStorage Storage(ComponentsStorageKey key)
		{
			return _componentsStorages[key];
		}
		
		public T RegisterComponentsStorage<T>(ComponentsStorageKey key, T storage) where T : ComponentsStorage
		{
			_componentsStorages[key] = storage;
			_componentsStoragesList.Add(storage);
			return storage;
		}
		#endregion ComponentsStorages

		#region Bootstrap
		async UniTask Bootstrap()
		{
			foreach (var bootstrapable in _bootstrapables)
			{
				await bootstrapable.Init(this);
			}
		}
		#endregion Bootstrap

		// #region Serialization
		// protected virtual void Serialize(BinaryWriter writer)
		// {
		// 	writer.Write(ControlCharacter);
		// 	writer.Write(_componentsStoragesList.Count);
		// 	foreach (var (key, storage) in _componentsStorages)
		// 	{
		// 		SerializersHelper.ToBytesStorageKey(key, writer);
		// 		storage.Serialize(writer);
		// 	}
		// 	writer.Write(ControlCharacter);
		// }
		//
		// protected virtual void Deserialize(BinaryReader reader)
		// {
		// 	Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		// 	var count = reader.ReadInt32();
		// 	for (var i = 0; i < count; i++)
		// 	{
		// 		ComponentsStorageKey key = new();
		// 		SerializersHelper.FromBytesStorageKey(ref key, reader);
		// 		var storage = _componentsStorages[key];
		// 		storage.Deserialize(this, reader);
		// 	}
		// 	Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		// }
		// #endregion Serialization
	}
}
