using System;
using KVD.ECS.Core.Components;

namespace KVD.ECS.Core.Helpers
{
	public readonly struct ComponentTypeHandle : IEquatable<ComponentTypeHandle>
	{
		public readonly ushort index;

		public ComponentTypeHandle(ushort value)
		{
			index = value;
		}

		public static ComponentTypeHandle From<T>() where T : unmanaged, IComponent
		{
			return new ComponentTypeHandle(TypeIndex<T>.Index);
		}

		public Type Type => s_types[index];

		public bool Equals(ComponentTypeHandle other)
		{
			return index == other.index;
		}
		public override bool Equals(object? obj)
		{
			return obj is ComponentTypeHandle other && Equals(other);
		}
		public override int GetHashCode()
		{
			return (int)index;
		}
		public static bool operator ==(ComponentTypeHandle left, ComponentTypeHandle right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ComponentTypeHandle left, ComponentTypeHandle right)
		{
			return !left.Equals(right);
		}

		static ushort s_nextTypeIndex;
		static Type[] s_types = new Type[1024];

		static class TypeIndex<T> where T : unmanaged, IComponent
		{
			public static ushort Index = Initialize();

			static ushort Initialize()
			{
				var index = s_nextTypeIndex++;
				if (index >= s_types.Length)
				{
					Array.Resize(ref s_types, s_types.Length * 2);
				}
				s_types[index] = typeof(T);
				return index;
			}
		}
	}
}
