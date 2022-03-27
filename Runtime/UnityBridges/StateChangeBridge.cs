using UnityEngine;

#nullable enable

namespace KVD.ECS.UnityBridges
{
	public class StateChangeBridge : StateMachineBehaviour
	{
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
		
		private static EcsToUnityLink? Link(Animator animator)
		{
			return animator.GetComponentInParent<EcsToUnityLink>();
		}
	}
}
