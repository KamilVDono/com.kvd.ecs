using KVD.ECS.Core.Components;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public readonly struct AnimatorStateEnter : IComponent
	{
		public readonly AnimatorStateInfo stateInfo;
		
		public AnimatorStateEnter(AnimatorStateInfo stateInfo)
		{
			this.stateInfo = stateInfo;
		}

		public void Dispose() {}
	}
}
