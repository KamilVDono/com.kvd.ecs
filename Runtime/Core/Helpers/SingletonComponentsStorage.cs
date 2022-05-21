using System;
using System.IO;
using KVD.ECS.Core.Components;
using KVD.ECS.Serializers;
using KVD.Utils.Extensions;
using UnityEngine.Assertions;

#nullable enable

namespace KVD.ECS.Core.Helpers
{
	public sealed class SingletonComponentsStorage
	{
		private const char ControlCharacter = 'S';
		private static readonly Type BucketGenericType = typeof(Bucket<>);
		
		private IBucket?[] _buckets;
		
		public SingletonComponentsStorage(int capacity)
		{
			_buckets = new IBucket[capacity];
		}
		
		public static int Index<T>() where T : struct, IComponent => Bucket<T>.Index;
		
		public bool Has<T>() where T : struct, IComponent
		{
			var index = Bucket<T>.Index;
			return !ReferenceEquals(_buckets[index], null);
		}
		
		public ref T Value<T>() where T : struct, IComponent
		{
			var index = Bucket<T>.Index;
			// By calling this we are certain that value exists so we don't need to handle nulls
#pragma warning disable 8602
#pragma warning disable 8600
			return ref ((Bucket<T>)_buckets[index]).value;
#pragma warning restore 8600
#pragma warning restore 8602
		}

		public ref T TryValue<T>(out bool has) where T : struct, IComponent
		{
			has = false;
			var index  = Bucket<T>.Index;
			if (_buckets[index] is not Bucket<T> bucket)
			{
				return ref Bucket<T>.dummyRefReturn;
			}
			
			has = true;
			return ref bucket.value;
		}
		
		public void Add<T>(T value) where T : struct, IComponent
		{
			var index = Bucket<T>.Index;
			if (_buckets[index] is Bucket<T> bucket)
			{
				bucket.value = value;
			}
			else
			{
				_buckets[index] = new Bucket<T>(value);
			}
		}
		
		public void Remove<T>() where T : struct, IComponent
		{
			var index = Bucket<T>.Index;
			_buckets[index] = null;
		}

		public void Remove(int index)
		{
			_buckets[index] = null;
		}
		
		public void Clear()
		{
			for (var i = 0; i < _buckets.Length; i++)
			{
				_buckets[i] = null;
			}
		}

		#region Serialization
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(ControlCharacter);
			writer.Write(_buckets.Length);

			for (var i = 0; i < _buckets.Length; i++)
			{
				var bucket = _buckets[i];
				if (bucket == null)
				{
					continue;
				}
				writer.Write(i);
				SerializersHelper.ToBytesType(bucket.TargetType, writer);
				bucket.Serialize(writer);
			}
			writer.Write(-1);

			writer.Write(_nextIndex);
			writer.Write(ControlCharacter);
		}

		public void Deserialize(BinaryReader reader)
		{
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			var length = reader.ReadInt32();
			if (_buckets.Length != length)
			{
				Array.Resize(ref _buckets, length);
			}
			
			var nextImplemented = reader.ReadInt32();
			for (var i = 0; i < length; i++)
			{
				if (i == nextImplemented)
				{
					var type       = SerializersHelper.FromBytesType(reader);
					var bucketType = BucketGenericType.MakeGenericType(type);
					var bucket     = (IBucket)Activator.CreateInstance(bucketType);
					_buckets[i] = bucket;
					bucket.Deserialize(i, reader);
					nextImplemented = reader.ReadInt32();
				}
				else
				{
					_buckets[i] = null;
				}
			}

			_nextIndex = reader.ReadInt32();
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		}
		#endregion Serialization

		public interface IBucket
		{
			public Type TargetType{ get; }

			public void Serialize(BinaryWriter writer);
			void Deserialize(int index, BinaryReader reader);
		}

		// ReSharper disable MissingBlankLines
		private static int _nextIndex;
		private sealed class Bucket<T> : IBucket where T : struct, IComponent
		{
			// This is intended
			// ReSharper disable once StaticMemberInGenericType
			// ReSharper disable once InconsistentNaming
			public static int Index = _nextIndex++;
			public static T dummyRefReturn;

			public T value;
			
			public Type TargetType => typeof(T);

			public Bucket()
			{
			}
			
			public Bucket(T value)
			{
				this.value = value;
			}

			public void Serialize(BinaryWriter writer)
			{
				value.Serialize(writer);
			}
			
			public void Deserialize(int index, BinaryReader reader)
			{
				Index = index;
				value = (T)value.Deserialize(reader);
			}
		}
		// ReSharper restore MissingBlankLines
	}
}
