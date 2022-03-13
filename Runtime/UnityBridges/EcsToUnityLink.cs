using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public class EcsToUnityLink : MonoBehaviour
	{
#nullable disable
		public World World{ get; private set; }
		public Entity Entity{ get; private set; } = Entity.Null;
		public ComponentsStorageKey StorageKey{ get; private set; }
		public ComponentsStorage Storage{ get; private set; }
#nullable enable

		public void Init(World world, Entity entity, ComponentsStorageKey storageKey, ComponentsStorage storage)
		{
			World      = world;
			Entity     = entity;
			StorageKey = storageKey;
			Storage    = storage;
		}

		private void OnDestroy()
		{
			World   = null;
			Storage = null;
			Entity = Entity.Null;
		}
	}
}
