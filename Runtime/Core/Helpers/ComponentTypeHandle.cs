using System;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.ComponentsLists;

#nullable enable

namespace KVD.ECS.Core.Helpers
{
	public readonly struct ComponentTypeHandle : IEquatable<ComponentTypeHandle>
	{
		public readonly ushort index;

		ComponentTypeHandle(ushort value)
		{
			index = value;
		}

		public static ComponentTypeHandle From<T>() where T : unmanaged, IComponent
		{
			return new ComponentTypeHandle(TypeIndex<T>.Index);
		}

		public Type Type => s_types[index];
		public ref readonly ComponentsListTypeInfo TypeInfo => ref s_typeInfos[index];

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

		// TODO: Make it eager initialized by all known types
		static ushort s_nextTypeIndex;
		static Type[] s_types = new Type[1024];
		static ComponentsListTypeInfo[] s_typeInfos = new ComponentsListTypeInfo[1024];

		static class TypeIndex<T> where T : unmanaged, IComponent
		{
			public static ushort Index = Initialize();

			static ushort Initialize()
			{
				var index = s_nextTypeIndex++;
				if (index >= s_types.Length)
				{
					Array.Resize(ref s_types, s_types.Length * 2);
					Array.Resize(ref s_typeInfos, s_typeInfos.Length * 2);
				}
				s_types[index] = typeof(T);
				s_typeInfos[index] = ComponentsListTypeInfo.From<T>(new ComponentTypeHandle(index));
				return index;
			}
		}
	}
}
