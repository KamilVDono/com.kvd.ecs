using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace KVD.ECS.Core.Systems
{
	public static class SaveSystem
	{
		public static int SaveSlot
		{
			get => PlayerPrefs.GetInt("SaveSlot", 0);
			set
			{
				PlayerPrefs.SetInt("SaveSlot", value);
				PlayerPrefs.Save();
			}
		}
		
		private static string SaveSlotPath => 
			Path.Combine(Application.persistentDataPath, $"SaveSlot{SaveSlot}", "save.save");

		public static void Load(ISceneLoader sceneLoader)
		{
			var savePath = SaveSlotPath;
			if (File.Exists(savePath))
			{
				Debug.LogError($"There is no save data for slot: {SaveSlot}. Reload aborted");
				return;
			}

			var loadBytesTask = File.ReadAllBytesAsync(savePath);
			var awaiter       = loadBytesTask.GetAwaiter();
			sceneLoader.BeforeSceneActivation += OnSceneLoaded;
			sceneLoader.Start();
			
			void OnSceneLoaded(Scene scene)
			{
				var       saveBytes    = awaiter.GetResult();
				var memoryStream = new MemoryStream(saveBytes, false);
				var reader       = new BinaryReader(memoryStream);

				var loadRequest = new GameObject("Load request", typeof(LoadRequest))
				{
					transform =
					{
						parent = scene.GetRootGameObjects()[0].transform,
					},
				};
				loadRequest.GetComponent<LoadRequest>().SetRequest(reader);
			}
		}

		public static void Save()
		{
			var savePath     = SaveSlotPath;
			var worldWrapper = WorldWrapper();
			if (worldWrapper.InitTask.Status != UniTaskStatus.Succeeded)
			{
				Debug.LogError($"Can not save not running world");
			}
			var       world          = worldWrapper.World;
			using var saveFileStream = new FileStream(savePath, FileMode.Create);
			using var binaryWriter   = new BinaryWriter(saveFileStream);
			world.Save(binaryWriter);
		}

		private static WorldWrapper WorldWrapper()
		{
			var roots        = SceneManager.GetActiveScene().GetRootGameObjects();
			var worldWrapper = roots.First(r => r.GetComponent<WorldWrapper>()).GetComponent<WorldWrapper>();
			return worldWrapper;
		}
	}
}
