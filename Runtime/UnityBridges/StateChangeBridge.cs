using KVD.ECS.Core.Components;
using UnityEngine;

#nullable enable

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
	
	public readonly struct AnimatorStateExit : IComponent
	{
		public readonly AnimatorStateInfo stateInfo;
		
		public AnimatorStateExit(AnimatorStateInfo stateInfo)
		{
			this.stateInfo = stateInfo;
		}
		
		public void Dispose() {}
	}
	
	public class StateChangeBridge : StateMachineBehaviour
	{
		private EcsToUnityLink? Link(Animator animator)
		{
			return animator.GetComponentInParent<EcsToUnityLink>();
		}
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var link = Link(animator);
			if (link == null)
			{
				return;
			}
			
			link.Storage.List<AnimatorStateEnter>().AddSingleFrame(link.Entity, new(stateInfo));
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var link = Link(animator);
			if (link == null)
			{
				return;
			}
			
			link.Storage.List<AnimatorStateExit>().AddSingleFrame(link.Entity, new(stateInfo));
		}
	}
}
