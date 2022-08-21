using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using KVD.ECS.Core;
using KVD.ECS.Core.Helpers;

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public static class StorageWorldSizeUtils
	{
		private const BindingFlags GetFieldFlag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;
		private static readonly Type BigBitmaskType = typeof(BigBitmask);

		public static float FullListSize(IComponentList list)
		{
			var type                      = list.GetType();
			var indexByEntityInfo         = type.GetField("_indexByEntity", GetFieldFlag)!;
			var entityByIndexInfo         = type.GetField("_entityByIndex", GetFieldFlag)!;
			var singleFrameComponentsInfo = type.GetField("_singleFrameComponents", GetFieldFlag)!;
			var entitiesMaskInfo          = type.GetField("_entitiesMask", GetFieldFlag)!;
			var masksInfo                 = BigBitmaskType.GetField("_masks", GetFieldFlag)!;
			
			var intSize       = Marshal.SizeOf<int>();
			var componentSize = ComponentSize(list);
			
			float size          = componentSize*list.Capacity;
			size += intSize*3;
			size += intSize*((int[])indexByEntityInfo.GetValue(list)).Length;
			size += intSize*((int[])entityByIndexInfo.GetValue(list)).Length;
			size += intSize*((List<int>)singleFrameComponentsInfo.GetValue(list)).Capacity;

			var bitmask = entitiesMaskInfo.GetValue(list);
			size += Marshal.SizeOf<ulong>()*((ulong[])masksInfo.GetValue(bitmask)).Length;
			return size;
		}
		
		public static int ComponentSize(IComponentList list)
		{
			var componentType = list.GetType().GetGenericArguments()[0];
			return Marshal.SizeOf(componentType);
		}
	}
}
