using System;
using UnityEngine;
using Object = UnityEngine.Object;

#nullable enable

namespace KVD.ECS.Components
{
	[Serializable]
	public readonly struct GameObjectWrapper : IEquatable<GameObjectWrapper>, IMonoComponent
	{
		public readonly GameObject value;
		
		public GameObjectWrapper(GameObject value)
		{
			this.value = value;
		}
		
		public static implicit operator GameObjectWrapper(GameObject go) => new(go);
		public static implicit operator GameObject(GameObjectWrapper wrapper) => wrapper.value;

		public bool Equals(GameObjectWrapper other)
		{
			return value.Equals(other.value);
		}
		public override bool Equals(object? obj)
		{
			return obj is GameObjectWrapper other && Equals(other);
		}
		public override int GetHashCode()
		{
			return value.GetHashCode();
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
			if (value)
			{
				Object.Destroy(value);
			}
		}
	}
}
