using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public class EcsToUnityLink : MonoBehaviour
	{
#nullable disable
		public World world;
		public Entity entity = Entity.Null;
		public ComponentsStorageKey storageKey;
		public ComponentsStorage storage;
#nullable enable

		// ReSharper disable ParameterHidesMember
		public void Init(World world, Entity entity, ComponentsStorageKey storageKey, ComponentsStorage storage)
		{
			this.world      = world;
			this.entity     = entity;
			this.storageKey = storageKey;
			this.storage    = storage;
		}
		// ReSharper restore ParameterHidesMember

		private void OnDestroy()
		{
			world   = null;
			storage = null;
			entity = Entity.Null;
		}
	}
}
