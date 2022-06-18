using KVD.ECS.Core.Components;
using UnityEngine.AddressableAssets;

namespace KVD.ECS.ComponentHelpers
{
	public struct PrefabWrapper : IComponent
	{
		public string prefabKey;

		public AssetReferenceGameObject AsReference()
		{
			return new(prefabKey);
		}
	}
}
