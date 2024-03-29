using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.UnityBridges;
using KVD.Utils.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KVD.ECS.PlayModeTests.Tests.PlayModeTests
{
	public class AnimatorAndStateChangedTests
	{
		const string SceneName = "AnimatorAndStateChangedScene";
		static readonly int NormalStateId = Animator.StringToHash("NormalState");
		static readonly int AngryStateId = Animator.StringToHash("AngryState");
		static readonly int TransitionToAngryId = Animator.StringToHash("TransitionToAngry");
		
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
			var countSystem = new CountStateChangesSystem();
			world.RegisterSystem(countSystem).GetAwaiter().GetResult();
			var triggerList = world.defaultStorage.List<SetAnimatorValue<AnimatorTrigger>>();
			
			var entityLink = Object.FindObjectOfType<EcsToUnityLink>();
			
			var animator = Object.FindObjectOfType<Animator>();
			var animatorState = animator.GetCurrentAnimatorStateInfo(0);
			Assert.AreEqual(NormalStateId, animatorState.shortNameHash);
			
			yield return null;
			
			triggerList.Add(entityLink.Entity, new(TransitionToAngryId, new()));
			
			yield return null;
			yield return null;
			yield return null;

			var stateEnter = countSystem.entered[0];
			Assert.AreEqual(NormalStateId, stateEnter.shortNameHash);
			stateEnter = countSystem.entered[1];
			Assert.AreEqual(AngryStateId, stateEnter.shortNameHash);
			var stateExit = countSystem.exited[0];
			Assert.AreEqual(NormalStateId, stateExit.shortNameHash);
			animatorState = animator.GetCurrentAnimatorStateInfo(0);
			Assert.AreEqual(AngryStateId, animatorState.shortNameHash);
		}

		public class CountStateChangesSystem : IterationsSystemBase<World>
		{
			ComponentsView<AnimatorStateEnter> _stateEnterView;
			ComponentsView<AnimatorStateExit> _stateExitView;

			public List<AnimatorStateInfo> entered = new();
			public List<AnimatorStateInfo> exited = new();

			protected override void InitReferences()
			{
				_stateEnterView = ComponentsViewBuilder.Create<AnimatorStateEnter>(World.defaultStorage);
				_stateExitView  = ComponentsViewBuilder.Create<AnimatorStateExit>(World.defaultStorage);
			}
			
			protected override void Update()
			{
				foreach (var enter in _stateEnterView)
				{
					entered.Add(enter.Get0().stateInfo);
				}
				foreach (var exit in _stateExitView)
				{
					exited.Add(exit.Get0().stateInfo);
				}
			}
		}
	}
}
