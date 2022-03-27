using System.Collections;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Generics;
using KVD.Utils.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KVD.ECS.PlayModeTests.Tests.PlayModeTests
{
	public class RegisterSystemsTests
	{
		private const string SceneName = "RegisterSystemsScene";
		
		[UnityTest]
		public IEnumerator Register()
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
			var world = wrapper.World;

			Assert.NotNull(world.System<WaitForAnimationExitSystem>());
			Assert.NotNull(world.System<WaitForAnimationEnterSystem>());
			Assert.NotNull(world.System<WaitForAnimationEnterSystem>());
		}
	}
}
