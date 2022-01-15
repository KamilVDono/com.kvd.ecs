using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KVD.ECS.Components
{
	public readonly struct PrefabRequestWrapper : IMonoComponent
	{
		private readonly AsyncOperationHandle<GameObject> _request;

		public PrefabRequestWrapper(AsyncOperationHandle<GameObject> request)
		{
			_request = request;
		}

		public void Dispose()
		{
			Addressables.ReleaseInstance(_request);
		}
	}
}