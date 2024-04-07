using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Entities.Allocators;
using KVD.ECS.Core.Helpers;
using KVD.Utils.DataStructures;
using KVD.Utils.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;

#nullable enable

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public unsafe class ComponentsStorage
	{
		#region DEBUG
		#if ENTITIES_NAMES
		readonly Dictionary<Entity, FixedString128Bytes> _debugNames = new(Entity.IndexComparer);
		#endif
		#endregion Debug
		UnsafeArray<ComponentListPtrSoft> _lists = new(128, Allocator.Persistent);

		readonly SingletonComponentsStorage _singletons = new(64);
		readonly List<int> _singleFrameSingletons = new(4);
		readonly List<NativeAllocation> _allocations = new(128);

		IEntityAllocator _entityAllocator;
	
		public Entity CurrentEntity{ get; private set; } = Entity.Null;
		public UnsafeArray<ComponentListPtrSoft> AllLists => _lists;
	
		public ComponentsStorage(IEntityAllocator? entityAllocator = null)
		{
			_entityAllocator = entityAllocator ?? new ContinuousEntitiesAllocator();
		}

		public void Dispose()
		{
			foreach (ref var listPtr in _lists)
			{
				if (listPtr.IsCreated)
				{
					listPtr.ToListPtr().AsList().Dispose();
				}
			}
			_lists.Dispose();

			foreach (var allocation in _allocations)
			{
				allocation.Free();
			}
			_allocations.Clear();

			_singletons.Clear();

			_singleFrameSingletons.Clear();
		}
	
		#region Lists
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetListPtr<T>(out ComponentListPtr<T> componentList) where T : unmanaged, IComponent
		{
			var soft = ListPtrSoft<T>();
			if (!soft.IsCreated)
			{
				componentList = default;
				return false;

			}
			componentList = soft.ToListPtr();
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr<T> ListPtr<T>(int initialSize = ComponentListConstants.InitialCapacity) where T : unmanaged, IComponent
		{
			return ListPtr(ComponentTypeHandle.From<T>(), initialSize).As<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr ListPtr(ComponentTypeHandle type, int initialSize = ComponentListConstants.InitialCapacity)
		{
			var listSoft = ListPtrSoft(type);
			return listSoft.ToListPtr(initialSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtrSoft<T> ListPtrSoft<T>() where T : unmanaged, IComponent
		{
			return ListPtrSoft(ComponentTypeHandle.From<T>()).As<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtrSoft ListPtrSoft(ComponentTypeHandle type)
		{
			var outOfIndex = type.index >= _lists.Length;
			if (outOfIndex || _lists[type.index].ptr == null)
			{
				if (outOfIndex)
				{
					UnsafeArrayExt.Resize(ref _lists, type.index + 8u);
				}
				var listMemory = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ComponentList>(),
					UnsafeUtility.AlignOf<ComponentList>(), Allocator.Persistent);
				UnsafeUtility.MemClear(listMemory, UnsafeUtility.SizeOf<ComponentList>());
				_lists[type.index] = new(listMemory, type.TypeInfo);
			}
			return _lists[type.index];
		}
		#endregion Lists
	
		#region Entities
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetAllocator(IEntityAllocator allocator)
		{
			if (CurrentEntity != Entity.Null)
			{
				return false;
			}
			_entityAllocator = allocator;
			return true;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity NextEntity()
		{
			CurrentEntity = _entityAllocator.Allocate();
			return CurrentEntity;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity NextEntity(FixedString128Bytes name)
		{
			CurrentEntity = _entityAllocator.Allocate();
			#if ENTITIES_NAMES
			name                       = name.IsEmpty ? new FixedString128Bytes(CurrentEntity.index.ToString()) : name;
			_debugNames[CurrentEntity] = name;
			#endif
			return CurrentEntity;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReturnEntity(Entity entity)
		{
			_entityAllocator.Return(entity);
		}
		#endregion Entities
		
		public void ClearSingleFrameEntities()
		{
			foreach (var listPtr in AllLists)
			{
				if (listPtr.IsCreated)
				{
					listPtr.ToListPtr().AsList().ClearSingleFrameEntities();
				}
			}
			for (var i = 0; i < _singleFrameSingletons.Count; i++)
			{
				_singletons.Remove(_singleFrameSingletons[i]);
			}
			_singleFrameSingletons.Clear();
		}
	
		#region Singleton components
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Singleton<T>() where T : struct, IComponent
		{
			return ref _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T SingletonOrNew<T>() where T : struct, IComponent
		{
			if (!_singletons.Has<T>())
			{
				_singletons.Add(default(T));
			}
			return ref _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SingletonOrDefault<T>() where T : struct, IComponent
		{
			return !_singletons.Has<T>() ? default : _singletons.Value<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T TrySingleton<T>(out bool has) where T : struct, IComponent
		{
			return ref _singletons.TryValue<T>(out has);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasSingleton<T>() where T : struct, IComponent
		{
			return _singletons.Has<T>();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Singleton<T>(T singleton, bool singleFrame = false) where T : unmanaged, IComponent
		{
			_singletons.Add(singleton);
			if (singleFrame)
			{
				_singleFrameSingletons.Add(SingletonComponentsStorage.Index<T>());
			}
			else
			{
				_singleFrameSingletons.Remove(SingletonComponentsStorage.Index<T>());
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSingleton<T>() where T : struct, IComponent
		{
			_singletons.Remove<T>();
			_singleFrameSingletons.Remove(SingletonComponentsStorage.Index<T>());
		}
		#endregion Singleton components

		public void AddAllocation(NativeAllocation allocation)
		{
			_allocations.Add(allocation);
		}
	
		public override string ToString()
		{
			// var estimatedSize = _lists.Length*32;
			// estimatedSize += 32;
			// var stringBuilder = new StringBuilder(estimatedSize);
			// if (_storageKey.HasValue)
			// {
			// 	#if DEBUG
			// 	stringBuilder.Append(ComponentsStorageKey.Name(_storageKey.Value));
			// 	#else
			// 	stringBuilder.Append(_storageKey.Value);
			// 	#endif
			// 	stringBuilder.Append("; ");
			// }
			// foreach (var listType in _lists)
			// {
			// 	stringBuilder.Append(listType.Type.Name);
			// 	stringBuilder.Append(", ");
			// }
			// if (_listsByType.Count > 0)
			// {
			// 	stringBuilder.Length -= 2;
			// }
			return "TODO"; // stringBuilder.ToString();
		}

		#region DEBUG
		[Conditional("ENTITIES_NAMES")]
		// ReSharper disable once RedundantAssignment
		public void Name(Entity entity, ref FixedString128Bytes name)
		{
			#if ENTITIES_NAMES
			if (_debugNames.TryGetValue(entity, out name))
			{
				return;
			}
			var indexName = entity.index.ToString();
			name = new FixedString128Bytes(indexName);
			#endif
		}
		
		[Conditional("STORAGES_CHECKS")]
		public void SafetyCheck()
		{
			#if ALLOCATORS_CHECKS
			_entityAllocator.AssertValidity(this);
			#endif
		}
		#endregion DEBUG
	}
}
