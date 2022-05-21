using System;
using KVD.ECS.Core;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Generics
{
	public readonly struct WaitForAnimationExit : IComponent, IEquatable<WaitForAnimationExit>
	{
		public readonly int animationName;
		public readonly Entity entity;
		public readonly ComponentsStorageKey storageKey;
		
		public WaitForAnimationExit(int animationName, Entity entity, ComponentsStorageKey storageKey)
		{
			this.animationName = animationName;
			this.entity        = entity;
			this.storageKey    = storageKey;
		}

		public static bool operator ==(WaitForAnimationExit left, AnimatorStateInfo stateInfo)
		{
			return left.animationName == stateInfo.shortNameHash;
		}
		
		public static bool operator !=(WaitForAnimationExit left, AnimatorStateInfo stateInfo)
		{
			return !(left == stateInfo);
		}

		public bool Equals(WaitForAnimationExit other)
		{
			return animationName == other.animationName;
		}
		public override bool Equals(object? obj)
		{
			return obj is WaitForAnimationExit other && Equals(other);
		}
		public override int GetHashCode()
		{
			return animationName;
		}
	}
}
