using System;
using System.IO;
using System.Runtime.InteropServices;
using KVD.ECS.Core;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using Unity.Collections;
using static KVD.Utils.Extensions.NativeContainersExt;

namespace KVD.ECS.Serializers
{
	public static class SerializersHelper
	{
		#region Write
		public static void ToBytesStruct<T>(T value, BinaryWriter writer) where T : unmanaged
		{
			var size = Marshal.SizeOf(value);
			// TODO: Use array pool
			var arr  = new byte[size];

			var ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(value, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			writer.Write(arr);
		}

		public static void ToBytesNativeArray<T>(NativeArray<T> values, BinaryWriter writer) where T : unmanaged
		{
			var allocator = ExtractAllocator(values);
			writer.Write((byte)allocator);
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return;
			}
			
			writer.Write(values.Length);

			foreach (var value in values)
			{
				ToBytesStruct(value, writer);
			}
		}
		
		public static void ToBytesComponentNativeArrayComponent<T>(NativeArray<T> values, BinaryWriter writer) where T : struct, IComponent
		{
			var allocator = ExtractAllocator(values);
			writer.Write((byte)allocator);
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return;
			}
			
			writer.Write(values.Length);

			var serializer = SerializersLibrary.Serializer<T>();
			foreach (var value in values)
			{
				serializer.WriteBytes(value, writer);
			}
		}

		public static void ToBytesNativeArrayEntity(NativeArray<Entity> values, BinaryWriter writer)
		{
			var allocator = ExtractAllocator(values);
			writer.Write((byte)allocator);
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return;
			}
			
			writer.Write(values.Length);

			foreach (var value in values)
			{
				ToBytesStruct(value, writer);
			}
		}

		public static void ToBytesStatelessInstance<T>(T _, BinaryWriter writer)
		{
			ToBytesStatelessInstance(typeof(T), writer);
		}
		
		public static void ToBytesStatelessInstance<T>(BinaryWriter writer)
		{
			ToBytesStatelessInstance(typeof(T), writer);
		}
		
		public static void ToBytesStatelessInstance(Type type, BinaryWriter writer)
		{
			var typeName = type.FullName!;
			writer.Write(typeName);
		}

		public static void ToBytesStorageKey(ComponentsStorageKey key, BinaryWriter writer)
		{
			var value = key.Value;
			writer.Write(value);
		}
		#endregion Write
		
		#region Read
		public static T FromMarshalBytes<T>(BinaryReader reader) where T : unmanaged
		{
			var structure = new T();
			var size      = Marshal.SizeOf(structure);
			var ptr       = Marshal.AllocHGlobal(size);
			var bytes     = reader.ReadBytes(size);

			Marshal.Copy(bytes, 0, ptr, size);

			structure = (T)Marshal.PtrToStructure(ptr, structure.GetType());
			Marshal.FreeHGlobal(ptr);

			return structure;
		}

		public static NativeArray<T> FromBytesNativeArray<T>(BinaryReader reader) where T : unmanaged
		{
			var allocator = (Allocator)reader.ReadByte();
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return new NativeArray<T>();
			}
			
			var length    = reader.ReadInt32();

			var array = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);

			for (var i = 0; i < length; i++)
			{
				array[i] = FromMarshalBytes<T>(reader);
			}

			return array;
		}
		
		public static NativeArray<T> FromBytesNativeArrayComponent<T>(BinaryReader reader) where T : struct, IComponent
		{
			var allocator = (Allocator)reader.ReadByte();
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return new NativeArray<T>();
			}
			
			var length    = reader.ReadInt32();

			var array = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);

			var serializer = SerializersLibrary.Serializer<T>();
			for (var i = 0; i < length; i++)
			{
				array[i] = serializer.ReadBytes(reader);
			}

			return array;
		}
		
		public static NativeArray<Entity> FromBytesNativeArrayEntity(BinaryReader reader)
		{
			var allocator = (Allocator)reader.ReadByte();
			if (allocator is Allocator.Invalid or Allocator.None)
			{
				return new NativeArray<Entity>();
			}
			
			var length    = reader.ReadInt32();

			var array = new NativeArray<Entity>(length, allocator, NativeArrayOptions.UninitializedMemory);

			for (var i = 0; i < length; i++)
			{
				array[i] = FromMarshalBytes<Entity>(reader);
			}

			return array;
		}
		
		public static Type FromBytesType(BinaryReader reader)
		{
			var typeName = reader.ReadString();
			return Type.GetType(typeName)!;
		}
		
		public static T FromBytesStatelessInstance<T>(BinaryReader reader)
		{
			return (T)FromBytesStatelessInstance(reader);
		}
		
		public static object FromBytesStatelessInstance(BinaryReader reader)
		{
			var type     = FromBytesType(reader);
			return Activator.CreateInstance(type)!;
		}
		
		public static ComponentsStorageKey FromBytesStorageKey(BinaryReader reader)
		{
			var value = reader.ReadInt32();
			return new ComponentsStorageKey(value);
		}
		#endregion Read
	}
}
