using System;
using UnityEngine;

namespace KVD.ECS
{
	[Serializable]
	public class ComponentsStorageKeyAuthoring
	{
		[SerializeField] private int _hash;
		
		public static implicit operator ComponentsStorageKey(ComponentsStorageKeyAuthoring wrapper) => new(wrapper._hash);
	}
}
