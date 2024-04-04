using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.ComponentsLists;

#nullable enable

namespace KVD.ECS.Core
{
	public readonly unsafe struct ComponentListPtrSoft : IEquatable<ComponentListPtrSoft>
	{
		public readonly ComponentList* ptr;
		public readonly ComponentsListTypeInfo typeInfo;

		public bool IsCreated => ptr != null && ptr->IsCreated;

		public ComponentListPtrSoft(void* ptr, in ComponentsListTypeInfo typeInfo)
		{
			this.ptr = (ComponentList*)ptr;
			this.typeInfo = typeInfo;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtrSoft<T> As<T>() where T : unmanaged, IComponent => new((ComponentListPtrSoft<T>*)ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr ToListPtr(int initialSize = ComponentListConstants.InitialCapacity)
		{
			if (!IsCreated)
			{
				*ptr = new ComponentList(typeInfo, initialSize);
			}
			return new ComponentListPtr(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList ToList(int initialSize = ComponentListConstants.InitialCapacity)
		{
			return ref ToListPtr().AsList();
		}

		public bool Equals(ComponentListPtrSoft other)
		{
			return ptr == other.ptr;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentListPtrSoft other && Equals(other);
		}
		public override int GetHashCode()
		{
			return unchecked((int)(long)ptr);
		}
		public static bool operator ==(ComponentListPtrSoft left, ComponentListPtrSoft right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ComponentListPtrSoft left, ComponentListPtrSoft right)
		{
			return !left.Equals(right);
		}
	}

	public readonly unsafe struct ComponentListPtrSoft<T> : IEquatable<ComponentListPtrSoft<T>> where T : unmanaged, IComponent
	{
		public readonly ComponentList<T>* ptr;

		public bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr != null && ptr->IsCreated;
		}

		public ComponentListPtrSoft(void* ptr)
		{
			this.ptr = (ComponentList<T>*)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtrSoft TypeLess() => new(ptr, ComponentsListTypeInfo.From<T>());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr<T> ToListPtr(int initialSize = ComponentListConstants.InitialCapacity)
		{
			if (!IsCreated)
			{
				*ptr = new ComponentList<T>(initialSize);
			}
			return new ComponentListPtr<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList<T> ToList(int initialSize = ComponentListConstants.InitialCapacity)
		{
			return ref ToListPtr().AsList();
		}

		public bool Equals(ComponentListPtrSoft<T> other)
		{
			return ptr == other.ptr;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentListPtrSoft<T> other && Equals(other);
		}
		public override int GetHashCode()
		{
			return unchecked((int)(long)ptr);
		}
		public static bool operator ==(ComponentListPtrSoft<T> left, ComponentListPtrSoft<T> right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ComponentListPtrSoft<T> left, ComponentListPtrSoft<T> right)
		{
			return !left.Equals(right);
		}
	}
}
