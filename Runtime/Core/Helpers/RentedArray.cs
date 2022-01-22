using System;
using System.Buffers;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;

#nullable enable

namespace KVD.ECS.Core.Helpers
{
	// TODO: There is size limit for array length in ArrayPool<T>.Shared, should be resolved via custom array pool
	public struct RentedArray<T> : IDisposable
	{
		public readonly T[] array;
		public int Length{ get; private set; }

		public RentedArray(int length)
		{
			Length = length;
			array  = length == 0 ? Array.Empty<T>() : ArrayPool<T>.Shared.Rent(length);
		}

		[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
		public ref T this[int index] => ref array[index];

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