using KVD.ECS.Core;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.UnityBridges;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#nullable enable

namespace KVD.ECS.ComponentHelpers
{
	[DefaultExecutionOrder(-1000)]
	public class RegisterEntity : MonoBehaviour
	{
#nullable disable
		[SerializeField] private AssetReferenceGameObject _assetReference;
		[SerializeField] private WorldWrapper _worldWrapper;
		[SerializeField] private ComponentsStorageKeyAuthoring _key;
		[SerializeField] private string _name;
#nullable enable
		private void Start()
		{
			var world = _worldWrapper.World;
			if (world.IsRestored)
			{
				return;
			}
			
			var myTransform = transform;
			var position    = myTransform.position;
			var rotation    = myTransform.rotation;
			var entityName  = string.IsNullOrWhiteSpace(_name) ? name : _name;
			
			Spawn(world, _key, _assetReference, position, rotation, entityName);
			
			Destroy(gameObject);
		}
		
		public static void Spawn(World world, ComponentsStorageKey key, AssetReferenceGameObject assetReference,
			Vector3 position, Quaternion rotation, string name)
		{
			var storage = world.Storage(key);
			var entity  = storage.NextEntity(name);

			Spawn(world, key, assetReference, position, rotation, name, storage, entity);
		}
		
		public static void Spawn(World world, ComponentsStorageKey key, AssetReferenceGameObject assetReference,
			Vector3 position, Quaternion rotation, string name, ComponentsStorage storage, Entity entity)
		{
			var prefabStorage = storage.List<PrefabWrapper>();
			prefabStorage.Add(entity, new() { prefabKey = (string)assetReference.RuntimeKey, });

			var instanceRequest = assetReference.InstantiateAsync(position, rotation);
			var instance        = instanceRequest.WaitForCompletion();
			#if ENTITIES_NAMES
			instance.name = $"{name} - Entity {entity.index}";
			#endif
			instance.AddComponent<EcsToUnityLink>().Init(world, entity, key, storage);

			SetupPrefabInstance(world, storage, entity, instanceRequest, instance, true);
		}

		public static void SetupPrefabInstance(World world, ComponentsStorage storage, Entity entity,
			AsyncOperationHandle<GameObject> request, GameObject instance, bool withInitialComponents)
		{
			var requestStorage = storage.List<PrefabRequestWrapper>();
			requestStorage.Add(entity, new(request));
			
			var goStorage = storage.List<GameObjectWrapper>();
			goStorage.Add(entity, instance);
			var transformStorage = storage.List<MonoComponentWrapper<Transform>>();
			transformStorage.Add(entity, new(instance.transform));

			var instanceRenderer = instance.GetComponent<Renderer>();
			if (instanceRenderer)
			{
				var rendererStorage = storage.List<MonoComponentWrapper<Renderer>>();
				rendererStorage.Add(entity, new(instanceRenderer));
			}

			if (!withInitialComponents)
			{
				return;
			}

			AddConvertableComponents(world, storage, entity, instance, true);

			var bridges = instance.GetComponentsInChildren<IUnityBridge>();
			foreach (var bridge in bridges)
			{
				bridge.Init();
			}
		}
		
		public static void AddConvertableComponents(World world, ComponentsStorage storage, Entity entity,
			GameObject instance, bool removeAuthoring)
		{
			var convertibles = instance.GetComponents<IConvertableMonoBehavior>();
			for (var i = convertibles.Length-1; i >= 0; i--)
			{
				var convertable        = convertibles[i];
				convertable.Register(entity, world, storage);
				if (removeAuthoring)
				{
					Destroy((Component)convertable);
				}
			}
		}
	}
}
