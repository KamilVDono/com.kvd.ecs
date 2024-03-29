using System;
using KVD.ECS.Core.Components;
using UnityEngine;
using Object = UnityEngine.Object;

#nullable enable

namespace KVD.ECS.ComponentHelpers
{
	[Serializable]
	public readonly struct GameObjectWrapper : IEquatable<GameObjectWrapper>, IMonoComponent
	{
		private readonly int _instanceId;
		public readonly GameObject Value => (GameObject)Resources.InstanceIDToObject(_instanceId);
		
		public GameObjectWrapper(GameObject value)
		{
			_instanceId = value.GetHashCode();
		}
		
		public static implicit operator GameObjectWrapper(GameObject go) => new(go);
		public static implicit operator GameObject(GameObjectWrapper wrapper) => wrapper.Value;

		public bool Equals(GameObjectWrapper other)
		{
			return _instanceId == other._instanceId;
		}
		public override bool Equals(object? obj)
		{
			return obj is GameObjectWrapper other && Equals(other);
		}
		public override int GetHashCode()
		{
			return _instanceId;
		}
		public static bool operator ==(GameObjectWrapper left, GameObjectWrapper right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(GameObjectWrapper left, GameObjectWrapper right)
		{ 
			return !left.Equals(right);
		}
		
		public void Dispose()
		{
			var value = Value;
			if (value)
			{
				Object.Destroy(value);
			}
		}
	}
}
