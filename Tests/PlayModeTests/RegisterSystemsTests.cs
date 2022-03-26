using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Generics;
using KVD.Utils.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace KVD.ECS.PlayModeTests.Tests.PlayModeTests
{
	public class RegisterSystemsTests
	{
		private const string SceneName = "RegisterSystemsScene";
		
		[UnityTest]
		public IEnumerator Register()
		{
			var scenes = AssetsFinder.Find<SceneAsset>();
			var scene = scenes.First(s => s.name == SceneName);
			EditorSceneManager.LoadSceneInPlayMode(AssetDatabase.GetAssetPath(scene), new(LoadSceneMode.Single));

			yield return null;
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
