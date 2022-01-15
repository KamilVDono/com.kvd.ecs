using KVD.ECS.UnityBridges;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KVD.ECS.Assets
{
	public class PrefabSpawner : MonoBehaviour
	{
#nullable disable
		[SerializeField] private AssetReferenceGameObject _prefab;
#nullable enable
		private AsyncOperationHandle<GameObject> _request;

		private void Awake()
		{
			_request           =  _prefab.InstantiateAsync(transform);
			_request.Completed += LinkToEcs;
		}
		
		private void LinkToEcs(AsyncOperationHandle<GameObject> request)
		{
			// Already destroyed
			if (this == null)
			{
				return;
			}

			var ecsToUnityLink = GetComponentInParent<EcsToUnityLink>();
			if (ecsToUnityLink == null)
			{
				return;
			}
			
			var bridges = request.Result.GetComponentsInChildren<IUnityBridge>();
			foreach (var bridge in bridges)
			{
				bridge.Init();
			}
		}

		private void OnDestroy()
		{
			Addressables.ReleaseInstance(_request);
		}
	}
}
