using KVD.ECS.Core.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace KVD.ECS.ComponentHelpers
{
	public readonly struct PrefabRequestWrapper : IMonoComponent
	{
		private readonly int _instanceId;

		public PrefabRequestWrapper(GameObject spawnedPrefab)
		{
			_instanceId = spawnedPrefab.GetHashCode();
		}

		public void Dispose()
		{
			Addressables.ReleaseInstance((GameObject)Resources.InstanceIDToObject(_instanceId));
		}
	}
}