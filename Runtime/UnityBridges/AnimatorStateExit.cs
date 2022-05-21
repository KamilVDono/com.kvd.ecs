using KVD.ECS.Core.Components;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public readonly struct AnimatorStateExit : IComponent
	{
		public readonly AnimatorStateInfo stateInfo;
		
		public AnimatorStateExit(AnimatorStateInfo stateInfo)
		{
			this.stateInfo = stateInfo;
		}
	}
}
