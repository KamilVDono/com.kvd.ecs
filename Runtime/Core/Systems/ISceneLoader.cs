using System;
using UnityEngine.SceneManagement;

#nullable enable

namespace KVD.ECS.Core.Systems
{
	public interface ISceneLoader
	{
		public event Action<Scene>? BeforeSceneActivation;

		public void Start();
	}
}
