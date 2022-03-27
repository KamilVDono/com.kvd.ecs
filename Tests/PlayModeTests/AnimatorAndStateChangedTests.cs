using System.Collections;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.UnityBridges;
using KVD.Utils.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KVD.ECS.PlayModeTests.Tests.PlayModeTests
{
	public class AnimatorAndStateChangedTests
	{
		private const string SceneName = "AnimatorAndStateChangedScene";
		private static readonly int NormalStateId = Animator.StringToHash("NormalState");
		private static readonly int AngryStateId = Animator.StringToHash("AngryState");
		private static readonly int TransitionToAngryId = Animator.StringToHash("TransitionToAngry");
		
		[UnityTest]
		public IEnumerator ChangeStateAndNotify()
		{
			foreach (var loadScene in PlaymodeTestsUtils.LoadTestScene(SceneName))
			{
				yield return loadScene;
			}
			
			var wrapper = Object.FindObjectOfType<WorldWrapper>();
			while (wrapper.InitTask.Status != UniTaskStatus.Succeeded)
			{
				yield return null;
			}
			var world       = wrapper.World;
			var triggerList = world.defaultStorage.List<SetAnimatorValue<AnimatorTrigger>>();
			var stateEnterList = world.defaultStorage.List<AnimatorStateEnter>();
			var stateExitList = world.defaultStorage.List<AnimatorStateExit>();

			var entityLink = Object.FindObjectOfType<EcsToUnityLink>();

			var animator = Object.FindObjectOfType<Animator>();
			var animatorState = animator.GetCurrentAnimatorStateInfo(0);
			Assert.AreEqual(NormalStateId, animatorState.shortNameHash);

			yield return null;
			world.defaultStorage.ClearSingleFrameEntities();

			triggerList.Add(entityLink.Entity, new(TransitionToAngryId, new()));
			
			yield return null;
			world.defaultStorage.ClearSingleFrameEntities();
			yield return null;
			
			var stateEnter = stateEnterList.Value(entityLink.Entity);
			Assert.AreEqual(AngryStateId, stateEnter.stateInfo.shortNameHash);
			var stateExit = stateExitList.Value(entityLink.Entity);
			Assert.AreEqual(NormalStateId, stateExit.stateInfo.shortNameHash);
			animatorState = animator.GetCurrentAnimatorStateInfo(0);
			Assert.AreEqual(AngryStateId, animatorState.shortNameHash);
		}
	}
}
