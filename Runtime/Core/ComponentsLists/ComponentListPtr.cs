using System;
using System.Runtime.CompilerServices;
using KVD.ECS.Core.Components;
using Unity.Collections.LowLevel.Unsafe;

#nullable enable

namespace KVD.ECS.Core
{
	public readonly unsafe struct ComponentListPtr : IEquatable<ComponentListPtr>
	{
		public readonly ComponentList* ptr;

		public ComponentListPtr(void* ptr)
		{
			this.ptr = (ComponentList*)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr<T> As<T>() where T : unmanaged, IComponent => new((ComponentList<T>*)ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList AsList() => ref UnsafeUtility.AsRef<ComponentList>(ptr);

		public bool Equals(ComponentListPtr other)
		{
			return ptr == other.ptr;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentListPtr other && Equals(other);
		}
		public override int GetHashCode()
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

		public ComponentListPtr(void* ptr)
		{
			this.ptr = (ComponentList<T>*)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentListPtr TypeLess() => new(ptr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref ComponentList<T> AsList() => ref UnsafeUtility.AsRef<ComponentList<T>>(ptr);

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
}
