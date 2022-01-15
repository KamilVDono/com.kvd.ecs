using System;
using KVD.ECS.Components;
using KVD.ECS.Entities;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Generics
{
	public readonly struct WaitForAnimationEnter : IComponent, IEquatable<WaitForAnimationEnter>
	{
		public readonly int animationName;
		public readonly Entity entity;
		public readonly ComponentsStorageKey storageKey;
		
		public WaitForAnimationEnter(int animationName, Entity entity, ComponentsStorageKey storageKey)
		{
			this.animationName = animationName;
			this.entity        = entity;
			this.storageKey    = storageKey;
		}

		public static bool operator ==(WaitForAnimationEnter left, AnimatorStateInfo stateInfo)
		{
			return left.animationName == stateInfo.shortNameHash;
		}
		
		public static bool operator !=(WaitForAnimationEnter left, AnimatorStateInfo stateInfo)
		{
			return !(left == stateInfo);
		}

		public bool Equals(WaitForAnimationEnter other)
		{
			return animationName == other.animationName;
		}
		public override bool Equals(object? obj)
		{
			return obj is WaitForAnimationEnter other && Equals(other);
		}
		public override int GetHashCode()
		{
			return animationName;
		}

		public void Dispose() {}
	}
}
