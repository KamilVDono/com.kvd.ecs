using System;
using UnityEngine.SceneManagement;

#nullable enable

namespace KVD.ECS.Core.Systems
{
	public class UnitySimpleSceneLoader : ISceneLoader
	{
		private readonly int _buildIndex;
		private readonly LoadSceneMode _loadMode;
		
		public event Action<Scene>? BeforeSceneActivation;

		public UnitySimpleSceneLoader(int buildIndex, LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			_buildIndex = buildIndex;
			_loadMode   = loadMode;
		}

		public void Start()
		{
			SceneManager.sceneLoaded += OnLoaded;
			SceneManager.LoadSceneAsync(_buildIndex, _loadMode);
		}

		private void OnLoaded(Scene scene, LoadSceneMode _)
		{
			SceneManager.sceneLoaded -= OnLoaded;
			var beforeSceneActivation = BeforeSceneActivation;
			BeforeSceneActivation = null;
			beforeSceneActivation?.Invoke(scene);
		}
	}
}
