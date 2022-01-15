using System;
using KVD.ECS.Components;

#nullable enable

namespace KVD.ECS
{
	public class SingletonComponentsStorage
	{
		private readonly IBucket?[] _buckets;
		
		public SingletonComponentsStorage(int capacity)
		{
			_buckets = new IBucket[capacity];
		}
		
		public bool Has<T>() where T : struct, ISingletonComponent
		{
			var index = Bucket<T>.Index;
			return !ReferenceEquals(_buckets[index], null);
		}
		
		public ref T Value<T>() where T : struct, ISingletonComponent
		{
			var index = Bucket<T>.Index;
			// By calling this we are certain that value exists so we don't need to handle nulls
#pragma warning disable 8602
#pragma warning disable 8600
			return ref ((Bucket<T>)_buckets[index]).value;
#pragma warning restore 8600
#pragma warning restore 8602
		}

		public ref T TryValue<T>(out bool has) where T : struct, ISingletonComponent
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
		
		public void Add<T>(T value) where T : struct, ISingletonComponent
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
		
		public void Remove<T>() where T : struct, ISingletonComponent
		{
			var index = Bucket<T>.Index;
			_buckets[index] = null;
		}

		public static int Index<T>() where T : struct, ISingletonComponent => Bucket<T>.Index;
		
		public void Remove(int index)
		{
			_buckets[index] = null;
		}

		public interface IBucket
		{
			public Type TargetType{ get; }
		}

		private static int _previousIndex = -1;
		
		private sealed class Bucket<T> : IBucket where T : struct, ISingletonComponent
		{
			// This is intended
			// ReSharper disable once StaticMemberInGenericType
			public static readonly int Index = ++_previousIndex;
			public static T dummyRefReturn;

			public T value;
			
			public Type TargetType => typeof(T);

			public Bucket(T value)
			{
				this.value = value;
			}
		}
	}
}
