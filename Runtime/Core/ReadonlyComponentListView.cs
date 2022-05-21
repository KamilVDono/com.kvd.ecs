using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;

#nullable enable

namespace KVD.ECS.Core
{
	public sealed class ReadonlyComponentListViewView<T> : IReadonlyComponentListView<T>
		where T : struct, IComponent
	{
		private readonly ComponentsStorage _storage;
	
		public BigBitmask EntitiesMask => BigBitmask.Empty; 
		public int Length => 0;
		public int EntitiesVersion => 0;
	
		public ReadonlyComponentListViewView(ComponentsStorage storage)
		{
			_storage = storage;
		}
		
		public bool Has(Entity entity)
		{
			return false;
		}
	
		public IReadonlyComponentListView<T> Sync()
		{
			return _storage.TryList<T>() ?? (IReadonlyComponentListView<T>)this;
		}
	}
}
