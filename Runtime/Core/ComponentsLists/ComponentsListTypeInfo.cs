using KVD.ECS.Core.Components;
using KVD.ECS.Core.Helpers;
using Unity.Collections.LowLevel.Unsafe;

namespace KVD.ECS.Core.ComponentsLists
{
	public readonly struct ComponentsListTypeInfo
	{
		public readonly ushort valueSize;
		public readonly ushort valueAlignment;
		public readonly ComponentTypeHandle typeHandle;

		public static ComponentsListTypeInfo From<T>() where T : unmanaged, IComponent
		{
			return new ComponentsListTypeInfo(
				(ushort)UnsafeUtility.SizeOf<T>(),
				(ushort)UnsafeUtility.AlignOf<T>(),
				ComponentTypeHandle.From<T>()
				);
		}

		public static ComponentsListTypeInfo From<T>(ComponentTypeHandle typeHandle) where T : unmanaged, IComponent
		{
			return new ComponentsListTypeInfo(
				(ushort)UnsafeUtility.SizeOf<T>(),
				(ushort)UnsafeUtility.AlignOf<T>(),
				typeHandle
				);
		}

		public ComponentsListTypeInfo(ushort valueSize, ushort valueAlignment, ComponentTypeHandle typeHandle)
		{
			this.valueSize = valueSize;
			this.valueAlignment = valueAlignment;
			this.typeHandle = typeHandle;
		}
	}
}
