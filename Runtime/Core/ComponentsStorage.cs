using System;
using System.Collections.Generic;
using System.Data;
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
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public unsafe class ComponentsStorage
	{
		#region DEBUG
		#if ENTITIES_NAMES
		readonly Dictionary<Entity, string> _debugNames = new(Entity.IndexComparer);
		#endif
		#endregion Debug
		UnsafeArray<ComponentListPtr> _lists = new(128, Allocator.Persistent);

		readonly SingletonComponentsStorage _singletons = new(64);
		readonly List<int> _singleFrameSingletons = new(4);
		readonly List<NativeAllocation> _allocations = new(128);

		IEntityAllocator _entityAllocator;
	
		public Entity CurrentEntity{ get; private set; } = Entity.Null;
		public UnsafeArray<ComponentListPtr> AllLists => _lists;
	
		public ComponentsStorage(IEntityAllocator? entityAllocator = null)
		{
			_entityAllocator = entityAllocator ?? new ContinuousEntitiesAllocator();
		}

		public void Dispose()
		{
			foreach (var listPtr in _lists)
			{
				if (listPtr.IsCreated)
				{
					listPtr.Dispose();
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
		public ComponentListPtr<T> TryGetListPtr<T>(out bool success) where T : unmanaged, IComponent
		{
			var typeHandle = ComponentTypeHandle.From<T>();
			if (_lists.Length > typeHandle.index)
			{
				var list = _lists[typeHandle.index];
				var componentList = list.As<T>();
				success = list.IsCreated;
				return componentList;
			}
			success = false;
			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList<T> TryGetList<T>(out bool success) where T : unmanaged, IComponent
		{
			return ref TryGetListPtr<T>(out success).AsList();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr<T> ListPtr<T>(int initialSize = ComponentListConstants.InitialCapacity) where T : unmanaged, IComponent
		{
			var key = ComponentTypeHandle.From<T>();
			var outOfIndex = key.index >= _lists.Length;
			if (outOfIndex || _lists[key.index].ptr == null)
			{
				if (outOfIndex)
				{
					UnsafeArrayExt.Resize(ref _lists, key.index + 8u);
				}
				var listMemory = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ComponentList<T>>(),
					UnsafeUtility.AlignOf<ComponentList<T>>(), Allocator.Persistent);
				UnsafeUtility.MemClear(listMemory, UnsafeUtility.SizeOf<ComponentList<T>>());
				_lists[key.index] = new(listMemory);
			}
			var list = _lists[key.index];
			var componentList = list.As<T>();
			componentList.Create(initialSize);
			return componentList;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList<T> List<T>(int initialSize = ComponentListConstants.InitialCapacity) where T : unmanaged, IComponent
		{
			return ref ListPtr<T>(initialSize).AsList();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr<T> ListPtrSoft<T>() where T : unmanaged, IComponent
		{
			var key = ComponentTypeHandle.From<T>();
			var outOfIndex = key.index >= _lists.Length;
			if (outOfIndex || _lists[key.index].ptr == null)
			{
				if (outOfIndex)
				{
					UnsafeArrayExt.Resize(ref _lists, key.index + 8u);
				}
				var listMemory = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ComponentList<T>>(),
					UnsafeUtility.AlignOf<ComponentList<T>>(), Allocator.Persistent);
				UnsafeUtility.MemClear(listMemory, UnsafeUtility.SizeOf<ComponentList<T>>());
				_lists[key.index] = new(listMemory);
			}
			return _lists[key.index].As<T>();
		}

		public ComponentListPtr ListPtrSoft(ComponentTypeHandle type)
		{
			var outOfIndex = type.index >= _lists.Length;
			if (outOfIndex || _lists[type.index].ptr == null)
			{
				if (outOfIndex)
				{
					UnsafeArrayExt.Resize(ref _lists, type.index + 8u);
				}
				var listMemory = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ComponentListTypeless>(),
					UnsafeUtility.AlignOf<ComponentListTypeless>(), Allocator.Persistent);
				UnsafeUtility.MemClear(listMemory, UnsafeUtility.SizeOf<ComponentListTypeless>());
				_lists[type.index] = new(listMemory);
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
		public Entity NextEntity(string name)
		{
			CurrentEntity = _entityAllocator.Allocate();
			#if ENTITIES_NAMES
			name                       = string.IsNullOrWhiteSpace(name) ? CurrentEntity.index.ToString() : name;
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
					listPtr.AsList().ClearSingleFrameEntities();
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
		public void Singleton<T>(T singleton, bool singleFrame = false) where T : struct, IComponent
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
		[Conditional("UNITY_EDITOR")]
		void CheckComponentType(Type type)
		{
			var componentInterface = typeof(IComponent);
			if (!type.IsValueType)
			{
				throw new ConstraintException($"Type {type} is not value type, that violate the contract");
			}
	
			if (!componentInterface.IsAssignableFrom(type))
			{
				throw new ConstraintException($"Type {type} is not implement {nameof(IComponent)} interface");
			}
		}
	
		[Conditional("ENTITIES_NAMES")]
		// ReSharper disable once RedundantAssignment
		public void Name(Entity entity, ref string name)
		{
			#if ENTITIES_NAMES
			var _ = _debugNames.TryGetValue(entity, out name) || (name = entity.index.ToString()) != null;
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
