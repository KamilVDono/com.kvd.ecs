using System;

namespace KVD.ECS.Entities
{
	public class CircularBufferEntitiesAllocator : IEntityAllocator
	{
		private readonly Entity[] _buffer;
		private int _nextEntity;

		public CircularBufferEntitiesAllocator(int size = 128)
		{
			_buffer = new Entity[size];
			for (var i = 0; i < size; i++)
			{
				_buffer[i] = i;
			}
		}

		public Entity Allocate()
		{
			var entity = _buffer[_nextEntity];
			_nextEntity = (_nextEntity+1)%_buffer.Length;
			return entity;
		}
		
		public void Return(Entity _) {}
		
		public void AssertValidity(ComponentsStorage storage)
		{
			#if !DEBUG
			return;
			#endif
			var nextEntity = _buffer[_nextEntity];
			if (storage.IsAlive(nextEntity))
			{
				throw new ApplicationException($"Next entity in {nameof(CircularBufferEntitiesAllocator)} for {storage} is still in usage");
			}
		}
	}
}
