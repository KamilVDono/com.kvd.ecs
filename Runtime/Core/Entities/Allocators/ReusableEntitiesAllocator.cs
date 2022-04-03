using System;
using System.IO;
using System.Text;

namespace KVD.ECS.Core.Entities.Allocators
{
	public class ReusableEntitiesAllocator : IEntityAllocator
	{
		private int _pointer = -1;
		private Entity _last = Entity.Null;
		private EntitiesPointer[] _buffer = new EntitiesPointer[256];

		public Entity Allocate()
		{
			if (_pointer == -1)
			{
				_last = _last.Next;
				return _last;
			}
			
			ref var entityPointer = ref _buffer[_pointer];
			if (entityPointer.count == 1)
			{
				--_pointer;
				return entityPointer.head;
			}
			
			--entityPointer.count;
			var entity = entityPointer.head;
			entityPointer.head = entity.Next;
			return entity;
		}
		
		public void Return(Entity e)
		{
			if (_pointer == -1)
			{
				_pointer   = 0;
				_buffer[0] = new() { head = e, count = 1, };
				return;
			}

			for (var i = _pointer; i >= 0; --i)
			{
				ref var entityPointer     = ref _buffer[i];
				
				var hasNext           = i > 0;
				var canMergeToNext    = hasNext && _buffer[i-1].head-e == 1;
				var canMergeToCurrent = entityPointer.head+entityPointer.count == e;
				
				// Merge both
				if (canMergeToNext && canMergeToCurrent)
				{
					Merge(i);
					return;
				}
				// Merge from left
				if (canMergeToCurrent)
				{
					entityPointer.count++;
					return;
				}
				// Merge from right
				if (entityPointer.head-e == 1)
				{
					entityPointer.count++;
					entityPointer.head = e;
					return;
				}
				//Insert before
				if (entityPointer.head > e)
				{
					InsertEntity(e, i+1);
					return;
				}
			}
			
			InsertEntity(e, _pointer+1);
		}
		
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(_pointer);
			writer.Write(_last.index);
			writer.Write(_buffer.Length);
			for (var i = 0; i < _buffer.Length; i++)
			{
				writer.Write(_buffer[i].head.index);
				writer.Write(_buffer[i].count);
			}
		}
		
		public void Deserialize(BinaryReader reader)
		{
			_pointer = reader.ReadInt32();
			_last    = reader.ReadInt32();
			
			var bufferSize = reader.ReadInt32();
			if (_buffer.Length != bufferSize)
			{
				Array.Resize(ref _buffer, bufferSize);
			}
			for (var i = 0; i < _buffer.Length; i++)
			{
				_buffer[i].head = reader.ReadInt32();
				_buffer[i].count = reader.ReadInt32();
			}
		}

		private void Merge(int position)
		{
			ref var entityPointer = ref _buffer[position];
			entityPointer.count += _buffer[position-1].count+1;
			
			var count = _pointer+1;
			Array.Copy(_buffer, position, _buffer, position-1, count-position);
			--_pointer;
		}
		
		private void InsertEntity(Entity e, int position)
		{
			if (position > _pointer)
			{
				if (position >= _buffer.Length)
				{
					Array.Resize(ref _buffer, _buffer.Length << 2);
				}
				_buffer[position] = new() { head = e, count = 1, };
			}
			else
			{
				var count = _pointer+1;
				Array.Copy(_buffer, position, _buffer, position+1, count-position);
			}
			++_pointer;
		}
		
		public void AssertValidity(ComponentsStorage storage)
		{
			#if !DEBUG
			return;
			#endif
			var sb = new StringBuilder();
			for (var e = 0; e <= _last; e++)
			{
				var inPool = HasInPool(e);
				var alive  = storage.IsAlive(e);
				if (inPool && alive)
				{
					sb.AppendLine($"Entity({e}) was released but is still alive");
				}
				else if (!inPool && !alive)
				{
					sb.AppendLine($"Entity({e}) wasn't released but is dead");
				}
			}
			if (sb.Length > 0)
			{
				throw new ApplicationException(sb.ToString());
			}
		}

		private bool HasInPool(Entity e)
		{
			var isInPool = false;
			var i        = 0;
			while (!isInPool && i <= _pointer)
			{
				var entityPointer = _buffer[i];
				isInPool = entityPointer.head <= e && e < entityPointer.head+entityPointer.count;
				++i;
			}
			return isInPool;
		}

		private struct EntitiesPointer
		{
			public Entity head;
			public int count;
		}
	}
}