using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using UnityEngine.Assertions;

namespace KVD.ECS.Core.Helpers
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public sealed class BigBitmask
	{
		private const char ControlCharacter = 'b';
		
		public const byte SingleMaskSize = 64;

		public static readonly BigBitmask Empty = new();
		
		private ulong[] _masks;

		public BigBitmask()
		{
			_masks = new ulong[1];
		}
		
		public BigBitmask(int initialCapacity)
		{
			_masks = new ulong[Bucket(initialCapacity)+1];
		}

		public bool Has(int index)
		{
			var bucket = Bucket(index);
			
			if (bucket >= _masks.Length)
			{
				return false;
			}

			var masked = _masks[bucket] & ((ulong)1 << BucketIndex(index));
			return masked > 0;
		}
		
		public void Set(int index, bool value)
		{
			var bucket = Bucket(index);

			if (bucket >= _masks.Length)
			{
				if (value)
				{
					// Length is bucket +1
					Resize(bucket+1);
				}
				else
				{
					return;
				}
			}

			if (value)
			{
				_masks[bucket] |= (ulong)1 << BucketIndex(index);
			}
			else
			{
				_masks[bucket] &= ~((ulong)1 << BucketIndex(index));
			}
		}
		
		public void EnsureCapacity(int sizeRequest)
		{
			var bucket = Bucket(sizeRequest);

			if (bucket >= _masks.Length)
			{
				Resize(bucket+1);
			}
		}
		
		public int Count()
		{
			var count = 0;
			foreach (var mask in _masks)
			{
				count += BitCount(mask);
			}
			return count;
		}

		public void Zero()
		{
			for (var i = 0; i < _masks.Length; i++)
			{
				_masks[i] = 0;
			}
		}

		public void All()
		{
			for (var i = 0; i < _masks.Length; i++)
			{
				_masks[i] =  ulong.MaxValue;
			}
		}
		
		public void CopyFrom(BigBitmask other)
		{
			var oldSize = _masks.Length;
			Resize(other._masks.Length);
			var i = 0;
			for (; i < other._masks.Length; i++)
			{
				_masks[i] = other._masks[i];
			}
			for (; i < oldSize; i++)
			{
				_masks[i] = 0;
			}
		}

		public void Intersect(BigBitmask other, bool valueIfResize)
		{
			if (other._masks.Length > _masks.Length)
			{
				var oldSize = _masks.Length;
				Resize(other._masks.Length);
				for (var i = oldSize; i < _masks.Length; i++)
				{
					_masks[i] = valueIfResize ? ulong.MaxValue : 0;
				}
			}
			
			{
				var i = 0;
				// We are at least other._masks size
				for (; i < other._masks.Length; i++)
				{
					_masks[i] &= other._masks[i];
				}
				// Zero all other
				for (; i < _masks.Length; i++)
				{
					_masks[i] = 0;
				}
			}
		}
		
		public void Union(BigBitmask other)
		{
			if (other._masks.Length > _masks.Length)
			{
				var oldSize = _masks.Length;
				Resize(other._masks.Length);
				for (var i = oldSize; i < _masks.Length; i++)
				{
					_masks[i] = 0;
				}
			}
			
			{
				var i = 0;
				// We are at least other._masks size
				for (; i < other._masks.Length; i++)
				{
					_masks[i] |= other._masks[i];
				}
			}
		}
		
		public void Exclude(BigBitmask other, bool valueIfResize)
		{
			if (other._masks.Length > _masks.Length)
			{
				var oldSize = _masks.Length;
				Resize(other._masks.Length);
				for (var i = oldSize; i < _masks.Length; i++)
				{
					_masks[i] = valueIfResize ? ulong.MaxValue : 0;
				}
			}
			// We are at least other._masks size
			for (var i = 0; i < other._masks.Length; i++)
			{
				_masks[i] &= ~other._masks[i];
			}
		}

		public Iterator GetEnumerator()
		{
			return new(_masks);
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(ControlCharacter);
			writer.Write(_masks.Length);
			writer.Write(ControlCharacter);
			foreach (var mask in _masks)
			{
				writer.Write(mask);
			}
			writer.Write(ControlCharacter);
		}
		
		public void Deserialize(BinaryReader reader)
		{
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			var length = reader.ReadInt32();
			Resize(length);
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
			for (var i = 0; i < length; i++)
			{
				_masks[i] = reader.ReadUInt64();
			}
			Assert.AreEqual(reader.ReadChar(), ControlCharacter);
		}

		private void Resize(int length)
		{
			if (_masks.Length >= length)
			{
				return;
			}
			Array.Resize(ref _masks, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Bucket(int index)
		{
			return index/SingleMaskSize;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int BucketIndex(int index)
		{
			return index%SingleMaskSize;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte BitCount(ulong value)
		{
			var result = value - ((value >> 1) & 0x5555555555555555UL);
			result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);
			return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
		}

		public ref struct Iterator
		{
			private readonly ulong[] _masks;
			private readonly int _length;
			private int _index;
			private int _bucket;
			private int _bucketIndex;

			public Iterator(ulong[] masks) : this()
			{
				_masks       = masks;
				_length      = _masks.Length*SingleMaskSize;
				_index       = -1;
				_bucketIndex = -1;
			}
		
			public int Current => _index;

			[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
			public bool MoveNext()
			{
				do
				{
					_index++;
					_bucketIndex++;
					if (_bucketIndex != SingleMaskSize)
					{
						continue;
					}
					_bucket++;
					_bucketIndex = 0;
				}
				while (_index < _length && (_masks[_bucket] & (ulong)1 << _bucketIndex) == 0);
				return _index < _length;
			}
		}
	}
}
