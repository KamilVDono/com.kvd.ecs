using KVD.ECS.Core;
using KVD.Utils.DataStructures;
using Unity.Collections.LowLevel.Unsafe;

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public static class StorageWorldSizeUtils
	{
		public static ulong FullListSize(in ComponentList list)
		{
			var listSize = (ulong)UnsafeUtility.SizeOf<ComponentList>();
			var maskSize = list.entitiesMask.BucketsSize();
			var singleFrameSize = (ulong)list.singleFrameComponents.Capacity * sizeof(int);
			var entityByIndexSize = (ulong)list.capacity * sizeof(int);
			var valuesSize = (ulong)list.capacity * list.typeInfo.valueSize;
			var indexByEntitySize = (ulong)list.indexByEntityCount * sizeof(int);
			return listSize + maskSize + singleFrameSize + entityByIndexSize + valuesSize + indexByEntitySize;
		}
		public static ulong FullListSize(in ComponentListPtrSoft listPtr)
		{
			return listPtr.IsCreated ? FullListSize(listPtr.ToList()) : 0ul;
		}
	}
}
