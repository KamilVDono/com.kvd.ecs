using System;
using System.Collections;
using System.IO;
using KVD.ECS.ComponentHelpers;
using KVD.ECS.Core;
using KVD.ECS.GeneralTests;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

#nullable disable

namespace KVD.ECS.PlayModeTests.Tests.PlayModeTests
{
	public class ComponentsStorageSerializationPlayMode
	{
		private const string Key = "Tests/AdditionalData/SparseList_Serialization_Prefab";
		
		[UnityTest]
		public IEnumerator MonoComponents()
		{
			World             worldStub  = new(Array.Empty<IBootstrapable>());
			ComponentsStorage oldStorage = new();

			foreach (var er in SpawnPrefab(worldStub, oldStorage, new(15, 5, 0), Quaternion.identity))
			{
				yield return er;
			}
			foreach (var er in SpawnPrefab(worldStub, oldStorage, new(15, 5, 0), Quaternion.Euler(50, 10, 0)))
			{
				yield return er;
			}
			foreach (var er in SpawnPrefab(worldStub, oldStorage, new(-15, -5, -60), Quaternion.identity))
			{
				yield return er;
			}

			// Wait 60 frames to load all prefabs
			for (var i = 0; i < 60; ++i)
			{
				yield return null;
			}
			
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);
			oldStorage.Serialize(writer);

			var oldStorageTransforms = oldStorage.List<MonoComponentWrapper<Transform>>();
			var originalTransforms   = ExtractTransformData(oldStorageTransforms);

			yield return null;
			
			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			ComponentsStorage newStorage = new();
			newStorage.Deserialize(worldStub, reader);
			
			// Wait 60 frames to load all prefabs
			for (var i = 0; i < 60; ++i)
			{
				yield return null;
			}
			
			var newStorageTransforms   = newStorage.List<MonoComponentWrapper<Transform>>();
			var deserializedTransforms = ExtractTransformData(newStorageTransforms);

			Assert.AreEqual(oldStorage.CurrentEntity, newStorage.CurrentEntity);
			AssertHelper.AreEqual(originalTransforms, deserializedTransforms);
			Assert.AreEqual(oldStorageTransforms.Value(0), oldStorageTransforms.Value(0));
			Assert.AreEqual(oldStorageTransforms.Value(1), oldStorageTransforms.Value(1));
			Assert.AreEqual(oldStorageTransforms.Value(2), oldStorageTransforms.Value(2));

			oldStorage.Destroy();
			newStorage.Destroy();
		}
		
		private static IEnumerable SpawnPrefab(World worldStub, ComponentsStorage storage, Vector3 position, Quaternion rotation)
		{
			var entity  = storage.NextEntity();
			
			var prefabStorage = storage.List<PrefabWrapper>();
			prefabStorage.Add(entity, new() { prefabKey = Key, });
			
			var request = Addressables.InstantiateAsync(Key, position, rotation);
			yield return request;
			var instance  = request.Result;
			RegisterEntity.SetupPrefabInstance(worldStub, storage, entity, request, instance, true);
		}

		private static TransformData[] ExtractTransformData(ComponentList<MonoComponentWrapper<Transform>> components)
		{
			var data = new TransformData[components.Length];
			for (var i = 0; i < components.Length; ++i)
			{
				var transform = components.DenseArray[i].value;
				var position  = (float3)transform.position;
				var rotation  = (quaternion)transform.rotation;
				data[i] = new(position, rotation);
			}
			return data;
		}

		private class TransformData : IEquatable<TransformData>
		{
			public readonly float3 position;
			public readonly quaternion rotation;

			public TransformData(float3 position, quaternion rotation)
			{
				this.position = position;
				this.rotation = rotation;
			}

			public bool Equals(TransformData other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}
				if (ReferenceEquals(this, other))
				{
					return true;
				}
				return position.Equals(other.position) && rotation.Equals(other.rotation);
			}
			
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}
				if (ReferenceEquals(this, obj))
				{
					return true;
				}
				if (obj.GetType() != GetType())
				{
					return false;
				}
				return Equals((TransformData)obj);
			}
			
			public override int GetHashCode()
			{
				return HashCode.Combine(position, rotation);
			}
			
			public static bool operator ==(TransformData left, TransformData right)
			{
				return Equals(left, right);
			}
			
			public static bool operator !=(TransformData left, TransformData right)
			{
				return !Equals(left, right);
			}
		}
	}
}
