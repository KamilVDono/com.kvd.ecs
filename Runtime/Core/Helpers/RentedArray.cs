using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

#nullable enable

namespace KVD.ECS.Core.Helpers
{
	// TODO: There is size limit for array length in ArrayPool<T>.Shared, should be resolved via custom array pool
	public struct RentedArray<T> : IDisposable
	{
		private const int InternalPoolMaxSize = 1_048_576;
		
		public readonly T[] array;
		public int Length{ get; private set; }
		public bool IsEmpty => Length < 1;

		public RentedArray(int length)
		{
			if (length is < 0 or > InternalPoolMaxSize)
			{
				throw new ArgumentException($"Length must be greater or equal zero but less than {InternalPoolMaxSize}",
					nameof(length));
			}
			Length = length;
			array  = length == 0 ? Array.Empty<T>() : ArrayPool<T>.Shared.Rent(length);
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
		public ref T this[int index] => ref array[index];

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining),]
		public void Add(T value)
		{
			array[Length] = value;
			++Length;
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining),]
		public void RemoveAt(int i)
		{
			array[i] = array[Length-1];
			--Length;
		}
		
		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),
		 MethodImpl(MethodImplOptions.AggressiveInlining),]
		public void ZeroSize()
		{
			Length = 0;
		}
		
		public Iterator GetEnumerator()
		{
			return new(array, Length);
		}

		public void Dispose()
		{
			if (Length > 0)
			{
				ArrayPool<T>.Shared.Return(array);
			}
			Length = 0;
		}

		public ref struct Iterator
		{
			private readonly T[] _array;
			private readonly int _length;
			private int _index;

			public Iterator(T[] array, int length)
			{
				_array  = array;
				_length = length;
				_index  = -1;
			}
		
			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public T Current => _array[_index];

			public bool MoveNext()
			{
				++_index;
				return _index < _length;
			}
		}
	}
}