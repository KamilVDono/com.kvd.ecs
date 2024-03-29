using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
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

	public unsafe struct ComponentListPtr : IEquatable<ComponentListPtr>
	{
		public ComponentListTypeless* ptr;

		public readonly bool IsCreated => ptr != null && ptr->IsCreated;

		public ComponentListPtr(void* ptr)
		{
			this.ptr = (ComponentListTypeless*)ptr;
		}

		public readonly ComponentListPtr<T> As<T>() where T : unmanaged, IComponent => new((ComponentList<T>*)ptr);

		public readonly ref ComponentListTypeless AsList() => ref UnsafeUtility.AsRef<ComponentListTypeless>(ptr);

		public void Dispose()
		{
			ptr->Dispose();
			UnsafeUtility.Free(ptr, Allocator.Persistent);
			ptr = null;
		}

		public readonly bool Equals(ComponentListPtr other)
		{
			return ptr == other.ptr;
		}
		public readonly override bool Equals(object? obj)
		{
			return obj is ComponentListPtr other && Equals(other);
		}
		public readonly override int GetHashCode()
		{
			return unchecked((int)(long)ptr);
		}
		public static bool operator ==(ComponentListPtr left, ComponentListPtr right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ComponentListPtr left, ComponentListPtr right)
		{
			return !left.Equals(right);
		}
	}

	public readonly unsafe struct ComponentListPtr<T> : IEquatable<ComponentListPtr<T>> where T : unmanaged, IComponent
	{
		public readonly ComponentList<T>* ptr;

		public bool IsCreated => ptr != null && ptr->IsCreated;

		public ComponentListPtr(void* ptr)
		{
			this.ptr = (ComponentList<T>*)ptr;
		}

		public ComponentListPtr TypeLess() => new(ptr);
		public ref ComponentList<T> AsList() => ref UnsafeUtility.AsRef<ComponentList<T>>(ptr);

		public void Create(int initialSize = ComponentListConstants.InitialCapacity)
		{
			if (!IsCreated)
			{
				*ptr = new ComponentList<T>(initialSize);
			}
		}

		public void Dispose()
		{
			TypeLess().Dispose();
		}

		public bool Equals(ComponentListPtr<T> other)
		{
			return ptr == other.ptr;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentListPtr<T> other && Equals(other);
		}
		public override int GetHashCode()
		{
			return unchecked((int)(long)ptr);
		}
		public static bool operator ==(ComponentListPtr<T> left, ComponentListPtr<T> right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ComponentListPtr<T> left, ComponentListPtr<T> right)
		{
			return !left.Equals(right);
		}
	}

	public unsafe struct ComponentListTypeless
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

		public void* defaultComponent;
		public ushort valueSize;
		public ushort valueAlignment;

		public bool IsCreated => values != null;

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
				throw new ArgumentException($"Entity [{entity}] is not present in ComponentListTypelessData");
			}
#endif
			return ((byte*)values)+indexByEntity[entity]*valueSize;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void* TryValue(int entity, out bool has)
		{
			var index = Index(entity);
			has = index >= 0;
			return has ? Value(entity) : defaultComponent;
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
				throw new ArgumentException($"Entity [{e}] already present in ComponentListTypelessData");
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
				UnsafeUtility.MemCpy(((byte*)values)+index*valueSize, value, valueSize);
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
				UnsafeUtility.MemSet(((byte*)values)+index*valueSize, 0, valueSize);
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
				UnsafeUtility.MemCpy(((byte*)values)+index*valueSize, value, valueSize);
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
			var destination = ((byte*)values)+index*valueSize;
			var source = ((byte*)values)+length*valueSize;
			UnsafeUtility.MemCpy(destination, source, valueSize);
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
			UnsafeUtility.MemCpy(((byte*)values)+(length-1)*valueSize, value, valueSize);
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

			UnsafeUtils.Resize(ref values, Allocator.Persistent, oldLength, capacity, valueSize, valueAlignment);
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
		public ComponentListTypeless typeless;

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
			typeless.entitiesVersion = 0;
			typeless.capacity      = Math.Max(capacity, 64);
			typeless.length        = 0;
			typeless.indexByEntity = (int*)UnsafeUtility.Malloc(typeless.capacity*UnsafeUtility.SizeOf<int>(),
				UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
			UnsafeUtils.Fill(typeless.indexByEntity, -1, typeless.capacity);
			typeless.indexByEntityCount = typeless.capacity;
			typeless.entityByIndex = (int*)UnsafeUtility.Malloc(typeless.capacity*UnsafeUtility.SizeOf<int>(),
				UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
			typeless.defaultComponent = UnsafeUtility.AddressOf(ref DefaultComponentProvider<T>.Default());
			typeless.valueSize = (ushort)UnsafeUtility.SizeOf<T>();
			typeless.valueAlignment = (ushort)UnsafeUtility.AlignOf<T>();
			typeless.values = (byte*)UnsafeUtility.Malloc(typeless.capacity*typeless.valueSize,
				typeless.valueAlignment, Allocator.Persistent);

			typeless.entitiesMask          = new((uint)typeless.capacity, Allocator.Persistent);
			typeless.singleFrameComponents = new(12, Allocator.Persistent);
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
