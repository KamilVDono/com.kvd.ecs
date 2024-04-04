using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.ComponentsLists;
using KVD.ECS.Core.Entities;
using KVD.Utils.DataStructures;
using KVD.Utils.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
#if LIST_PROFILER_MARKERS
using Unity.Profiling;
#endif

#nullable enable

namespace KVD.ECS.Core
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public static class ComponentListConstants
	{
		public const int InitialCapacity = 128;
		
#if LIST_PROFILER_MARKERS
		public static readonly ProfilerMarker EnsureSizeMarker = new("SparseList.EnsureSize");
		public static readonly ProfilerMarker AddMarker = new("SparseList.Add");
		public static readonly ProfilerMarker BulkAddMarker = new("SparseList.BulkAdd");
		public static readonly ProfilerMarker RemoveMarker = new("SparseList.Remove");
#endif
	}

	public unsafe struct ComponentList
	{
		public UnsafeBitmask entitiesMask;
		public int entitiesVersion;

		public UnsafeList<int> singleFrameComponents;

		public int capacity;
		public int length;
		public int indexByEntityCount;

		public int* entityByIndex;
		public void* values;
		public int* indexByEntity;

		public ComponentsListTypeInfo typeInfo;

		public bool IsCreated => values != null;

		public ComponentList(in ComponentsListTypeInfo typeInfo, int capacity = ComponentListConstants.InitialCapacity)
		{
			this.typeInfo = typeInfo;

			this.capacity = Math.Max(capacity, 64);
			entitiesVersion = 0;
			length          = 0;
			indexByEntity = (int*)UnsafeUtility.Malloc(capacity*UnsafeUtility.SizeOf<int>(),
				UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
			UnsafeUtils.Fill(indexByEntity, -1, capacity);
			indexByEntityCount = capacity;
			entityByIndex = (int*)UnsafeUtility.Malloc(capacity*UnsafeUtility.SizeOf<int>(),
				UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
			values = (byte*)UnsafeUtility.Malloc(capacity*typeInfo.valueSize, typeInfo.valueAlignment, Allocator.Persistent);

			entitiesMask          = new((uint)capacity, Allocator.Persistent);
			singleFrameComponents = new(12, Allocator.Persistent);
		}

		public void Dispose()
		{
			// TODO: Dispose components
			// for (var i = 0; i < length; ++i)
			// {
			// 	((IComponent*)values)[i].Dispose();
			// }

			length = 0;
			capacity = 0;

			UnsafeUtility.Free(indexByEntity, Allocator.Persistent);
			indexByEntity = null;
			UnsafeUtility.Free(entityByIndex, Allocator.Persistent);
			entityByIndex = null;
			UnsafeUtility.Free(values, Allocator.Persistent);
			values = null;
			singleFrameComponents.Dispose();
			entitiesMask.Dispose();

			entitiesVersion = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void* Value(int entity)
		{
#if LIST_CHECKS
			if (!Has(entity))
			{
				throw new ArgumentException($"Entity [{entity}] is not present in ComponentListData");
			}
#endif
			return ((byte*)values)+indexByEntity[entity]*typeInfo.valueSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void* TryValue(int entity, out bool has)
		{
			var index = Index(entity);
			has = index >= 0;
			return has ? Value(entity) : typeInfo.defaultComponent;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Has(Entity entity)
		{
			return Index(entity) >= 0;
		}

		public void Add(Entity e, void* value)
		{
#if LIST_PROFILER_MARKERS
			using var marker = ComponentListConstants.AddMarker.Auto();
#endif
#if LIST_CHECKS
			if (Has(e))
			{
				throw new ArgumentException($"Entity [{e}] already present in ComponentListData");
				// TODO: better message
				//throw new ArgumentException($"Entity [{e}] already present in ComponentList<{typeof(T).Name}>");
			}
#endif

			InternalAddSafe(e, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrReplace(Entity entity, void* value)
		{
			// Just replace value
			var index = Index(entity);
			if (index > -1)
			{
				UnsafeUtility.MemCpy(((byte*)values)+index*typeInfo.valueSize, value, typeInfo.valueSize);
			}
			// Create new value
			else
			{
				InternalAddSafe(entity, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BulkAdd(UnsafeArray<Entity> entities)
		{
#if LIST_PROFILER_MARKERS
			using var marker = ComponentListConstants.BulkAddMarker.Auto();
#endif

			var startSize      = length;
			var entitiesLength = entities.LengthInt;
			length += entitiesLength;
			EnsureSize(entities[entities.Length-1]);

			for (var i = 0u; i < entities.Length; i++)
			{
				var index       = startSize+i;
				var entityIndex = entities[i].index;
				indexByEntity[entityIndex] = (int)index;
				entityByIndex[index]       = entities[i];
				UnsafeUtility.MemSet(((byte*)values)+index*typeInfo.valueSize, 0, typeInfo.valueSize);
				entitiesMask.Up((uint)entityIndex);
			}

			entitiesVersion++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BulkAdd(UnsafeArray<Entity> entities, void* value)
		{
#if LIST_PROFILER_MARKERS
			using var marker = ComponentListConstants.BulkAddMarker.Auto();
#endif

			var startSize      = length;
			var entitiesLength = entities.LengthInt;
			length += entitiesLength;
			EnsureSize(entities[entities.Length-1]);

			for (var i = 0u; i < entities.Length; i++)
			{
				var index       = startSize+i;
				var entityIndex = entities[i].index;
				indexByEntity[entityIndex] = (int)index;
				entityByIndex[index]       = entities[i];
				UnsafeUtility.MemCpy(((byte*)values)+index*typeInfo.valueSize, value, typeInfo.valueSize);
				entitiesMask.Up((uint)entityIndex);
			}

			entitiesVersion++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddSingleFrame(Entity e, void* value)
		{
			Add(e, value);
			singleFrameComponents.Add(e);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearSingleFrameEntities()
		{
			foreach (var singleFrameComponent in singleFrameComponents)
			{
				Remove(singleFrameComponent);
			}
			singleFrameComponents.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int Index(Entity e)
		{
			return Index(e.index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int Index(int e)
		{
			return e < indexByEntityCount ? indexByEntity[e] : -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(int entity)
		{
#if LIST_PROFILER_MARKERS
			using var marker = ComponentListConstants.RemoveMarker.Auto();
#endif
			var index = indexByEntity[entity];
			if (index <= -1)
			{
				// The entity is not present in array
				return false;
			}
			// TODO: Dispose component
			//_state.values[index].Dispose();

			entitiesVersion++;

			--length;
			if (length == 0)
			{
				indexByEntity[entity] = -1;
				entitiesMask.Down((uint)entity);
				return true;
			}
			// _length was decreased so it's pointing to last element
			// Swap last value with removed one
			var destination = ((byte*)values)+index*typeInfo.valueSize;
			var source = ((byte*)values)+length*typeInfo.valueSize;
			UnsafeUtility.MemCpy(destination, source, typeInfo.valueSize);
			// Remove entity from list
			entitiesMask.Down((uint)entity);

			var entityIndexToSwap = entityByIndex[length];
			indexByEntity[entityIndexToSwap] = index;
			entityByIndex[index]             = entityIndexToSwap;

			indexByEntity[entity] = -1;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int requestedCapacity, Entity lastEntity)
		{
			IncreaseSizeDense(requestedCapacity);
			IncreaseSizeEntities(lastEntity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void InternalAddSafe(Entity e, void* value)
		{
			++length;
			EnsureSize(e);
			var entityIndex = e.index;
			indexByEntity[entityIndex] = length-1;
			entityByIndex[length-1] = entityIndex;
			UnsafeUtility.MemCpy(((byte*)values)+(length-1)*typeInfo.valueSize, value, typeInfo.valueSize);
			entitiesMask.Up((uint)entityIndex);

			entitiesVersion++;
		}

		#region Resize
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureSize(Entity lastEntity)
		{
#if LIST_PROFILER_MARKERS
			using var marker = ComponentListConstants.EnsureSizeMarker.Auto();
#endif

			IncreaseSizeDense(length);
			IncreaseSizeEntities(lastEntity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncreaseSizeDense(int requireSize)
		{
			if (requireSize <= capacity)
			{
				return;
			}

			var oldLength = capacity;
			while (capacity < requireSize)
			{
				capacity <<= 2;
			}

			UnsafeUtils.Resize(ref values, Allocator.Persistent, oldLength, capacity, typeInfo.valueSize, typeInfo.valueAlignment);
			UnsafeUtils.Resize(ref entityByIndex, Allocator.Persistent, oldLength, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncreaseSizeEntities(Entity lastEntity)
		{
			var sizeRequest = lastEntity.index+1;
			var oldSize     = indexByEntityCount;

			if (sizeRequest <= oldSize)
			{
				return;
			}

			--sizeRequest;
			sizeRequest <<= 2;
			while (sizeRequest < oldSize)
			{
				sizeRequest <<= 2;
			}

			indexByEntityCount = sizeRequest;
			UnsafeUtils.Resize(ref indexByEntity, Allocator.Persistent, oldSize, sizeRequest);
			UnsafeUtils.Fill(indexByEntity+oldSize, -1, sizeRequest-oldSize);
			entitiesMask.EnsureCapacity((uint)sizeRequest);
		}
		#endregion Resize
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public unsafe struct ComponentList<T> where T : unmanaged, IComponent
	{
		public ComponentList typeless;

		public readonly bool IsCreated => typeless.IsCreated;

		public readonly UnsafeBitmask EntitiesMask => typeless.entitiesMask;
		public readonly int EntitiesVersion => typeless.entitiesVersion;

		public readonly int IndexByEntityCount => typeless.indexByEntityCount;
		public readonly int* IndexByEntity => typeless.indexByEntity;
		public readonly int* EntityByIndex => typeless.entityByIndex;
		public readonly int Capacity => typeless.capacity;
		public readonly int Length => typeless.length;
		public readonly T* DenseArray => (T*)typeless.values;
		public readonly Span<T> ValidDenseArray => new(typeless.values, Length);

		public ComponentList(int capacity = ComponentListConstants.InitialCapacity)
		{
			typeless = new(ComponentsListTypeInfo.From<T>(), capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() => typeless.Dispose();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Has(Entity entity) => typeless.Has(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T Value(int entity) => ref UnsafeUtility.AsRef<T>(typeless.Value(entity));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref T TryValue(int entity, out bool has) => ref UnsafeUtility.AsRef<T>(typeless.TryValue(entity, out has));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Entity e, T value) => typeless.Add(e, UnsafeUtility.AddressOf(ref value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrReplace(Entity entity, T value) => typeless.AddOrReplace(entity, UnsafeUtility.AddressOf(ref value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BulkAdd(UnsafeArray<Entity> entities) => typeless.BulkAdd(entities);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BulkAdd(UnsafeArray<Entity> entities, T value) => typeless.BulkAdd(entities, UnsafeUtility.AddressOf(ref value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddSingleFrame(Entity e, T value) => typeless.AddSingleFrame(e, UnsafeUtility.AddressOf(ref value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearSingleFrameEntities() => typeless.ClearSingleFrameEntities();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(int entity) => typeless.Remove(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int capacity, Entity lastEntity) => typeless.EnsureCapacity(capacity, lastEntity);
	}
}
