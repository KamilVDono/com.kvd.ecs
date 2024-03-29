using KVD.ECS.Core.Components;
using Unity.Collections;
using UnityEngine.AddressableAssets;

namespace KVD.ECS.ComponentHelpers
{
	public struct PrefabWrapper : IComponent
	{
		public FixedString512Bytes prefabKey;

		public PrefabWrapper(AssetReferenceGameObject assetReference)
		{
			prefabKey = new FixedString512Bytes(assetReference.RuntimeKey.ToString());
		}

		public readonly AssetReferenceGameObject AsReference()
		{
			return new(prefabKey.ToString());
		}
	}
}
