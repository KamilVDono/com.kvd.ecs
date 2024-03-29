using KVD.ECS.Core;
using KVD.Utils.DataStructures;
using Unity.Collections.LowLevel.Unsafe;

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public static class StorageWorldSizeUtils
	{
		public static ulong FullListSize(in ComponentListTypeless list)
		{
			var listSize = (ulong)UnsafeUtility.SizeOf<ComponentListTypeless>();
			var maskSize = list.entitiesMask.BucketsSize();
			var singleFrameSize = (ulong)list.singleFrameComponents.Capacity * sizeof(int);
			var entityByIndexSize = (ulong)list.capacity * sizeof(int);
			var valuesSize = (ulong)list.capacity * list.valueSize;
			var indexByEntitySize = (ulong)list.indexByEntityCount * sizeof(int);
			return listSize + maskSize + singleFrameSize + entityByIndexSize + valuesSize + indexByEntitySize;
		}
		public static ulong FullListSize(in ComponentListPtr listPtr)
		{
			return listPtr.IsCreated ? FullListSize(listPtr.AsList()) : 0ul;
		}
	}
}
