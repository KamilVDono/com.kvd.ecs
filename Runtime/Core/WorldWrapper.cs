using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using KVD.Utils.Attributes;
using UnityEngine;

namespace KVD.ECS.Core
{
	[DefaultExecutionOrder(-1500)]
	public class WorldWrapper : MonoBehaviour
	{
#nullable disable
		[SerializeField, SerializableInterface(typeof(IBootstrapable)),]
		protected MonoBehaviour[] _bootstrapables;
		
		public World World{ get; private set; }
#nullable enable
		private UniTask _initTask;
		public UniTask InitTask => _initTask;

		private async void Awake()
		{
			try
			{
				World     = CreateWorld();
				_initTask = World.Initialize();
				await _initTask;
				// TODO: FIX ME
				// if (!SavesSystem.IsLoading)
				// {
				// 	await World.Initialize();
				// }
				// else
				// {
				// 	SavesSystem.IsLoading = false;
				// 	using var reader = SavesSystem.CurrentSaveReader();
				// 	await World.Restore(reader);
				// }
			}
			catch (Exception e)
			{
				Debug.LogException(e, this);
				throw;
			}
		}

		protected virtual World CreateWorld()
		{
			return new(_bootstrapables.OfType<IBootstrapable>().ToArray());
		}

		private async void OnDestroy()
		{
			await World.Destroy();
		}

		private void Update()
		{
			if (_initTask.Status != UniTaskStatus.Succeeded)
			{
				return;
			}
			World.Update();
		}
	}
}
