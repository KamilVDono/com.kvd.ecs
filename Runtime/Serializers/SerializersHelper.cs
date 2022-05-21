using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

using KVD.ECS.Core;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Helpers;

namespace KVD.ECS.Serializers
{
	public static class SerializersHelper
	{
		#region Write
		private static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Shared;

		public static void ToBytesStruct<T>(T value, BinaryWriter writer) where T : unmanaged
		{
			if (value is bool boolValue)
			{
				writer.Write(boolValue);
				return;
			}
			var size = Marshal.SizeOf(value);
			var array = ByteArrayPool.Rent(size);

			var pointer = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(value, pointer, true);
			Marshal.Copy(pointer, array, 0, size);
			Marshal.FreeHGlobal(pointer);
			writer.Write(array, 0, size);

			ByteArrayPool.Return(array);
		}

		public static void ToBytesRentedArray<T>(RentedArray<T> values, BinaryWriter writer) where T : unmanaged
		{
			writer.Write(values.Length);

			foreach (var value in values)
			{
				ToBytesStruct(value, writer);
			}
		}

		public static void ToBytesComponentRentedArray<T>(RentedArray<T> values, BinaryWriter writer)
			where T : struct, IComponent
		{
			writer.Write(values.Length);

			foreach (var value in values)
			{
				value.Serialize(writer);
			}
		}

		public static void ToBytesArray<T>(T[] values, BinaryWriter writer) where T : unmanaged
		{
			writer.Write(values.Length);

			foreach (var value in values)
			{
				ToBytesStruct(value, writer);
			}
		}

		public static void ToBytesComponentArray<T>(T[] values, BinaryWriter writer) where T : struct, IComponent
		{
			writer.Write(values.Length);

			foreach (var value in values)
			{
				value.Serialize(writer);
			}
		}

		public static void ToBytesStatelessInstance(object obj, BinaryWriter writer)
		{
			ToBytesType(obj.GetType(), writer);
		}

		public static void ToBytesStatelessInstance<T>(BinaryWriter writer)
		{
			ToBytesType(typeof(T), writer);
		}

		public static void ToBytesType(Type type, BinaryWriter writer)
		{
			var typeName = $"{type.FullName!}, {type.Assembly.FullName}";
			writer.Write(typeName);
		}

		public static void ToBytesStorageKey(ComponentsStorageKey key, BinaryWriter writer)
		{
			var value = key.Value;
			writer.Write(value);
		}
		#endregion Write

		#region Read
		public static void FromBytesStruct<T>(ref T val, BinaryReader reader) where T : unmanaged
		{
			var structure = new T();
			var size = Marshal.SizeOf(structure);
			var ptr = Marshal.AllocHGlobal(size);
			var bytes = reader.ReadBytes(size);

			Marshal.Copy(bytes, 0, ptr, size);

			val = (T)Marshal.PtrToStructure(ptr, structure.GetType());
			Marshal.FreeHGlobal(ptr);
		}

		public static void FromBytesRentedArray<T>(ref RentedArray<T> rentedArray, BinaryReader reader) where T : unmanaged
		{
			var length = reader.ReadInt32();
			rentedArray = new(length);

			for (var i = 0; i < length; i++)
			{
				FromBytesStruct(ref rentedArray[i], reader);
			}
		}

		public static void FromBytesRentedArrayComponent<T>(ref RentedArray<T> rentedArray, BinaryReader reader)
			where T : struct, IComponent
		{
			var length = reader.ReadInt32();
			rentedArray = new(length);

			for (var i = 0; i < length; i++)
			{
				var val = new T();
				val = (T)val.Deserialize(reader);
				rentedArray[i] = val;
			}
		}

		public static void FromBytesArray<T>(ref T[] val, BinaryReader reader) where T : unmanaged
		{
			var length = reader.ReadInt32();
			val = new T[length];

			for (var i = 0; i < length; i++)
			{
				FromBytesStruct<T>(ref val[i], reader);
			}
		}

		public static void FromBytesArrayComponent<T>(ref T[] array, BinaryReader reader) where T : struct, IComponent
		{
			var length = reader.ReadInt32();
			array = new T[length];

			for (var i = 0; i < length; i++)
			{
				var val = new T();
				val.Deserialize(reader);
				array[i] = val;
			}
		}

		public static void FromBytesStatelessInstance<T>(ref T instance, BinaryReader reader)
		{
			object obj = null;
			FromBytesStatelessInstance(ref obj, reader);
			instance = (T)obj;
		}

		public static void FromBytesStatelessInstance(ref object obj, BinaryReader reader)
		{
			var type = FromBytesType(reader);
			obj = Activator.CreateInstance(type)!;
		}

		public static Type FromBytesType(BinaryReader reader)
		{
			var typeName = reader.ReadString();
			return Type.GetType(typeName)!;
		}

		public static void FromBytesStorageKey(ref ComponentsStorageKey key, BinaryReader reader)
		{
			var value = reader.ReadInt32();
			key = new(value);
		}
		#endregion Read
	}
}
