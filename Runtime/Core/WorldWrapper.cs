﻿using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core.Systems;
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
		UniTask _initTask;
		public UniTask InitTask => _initTask;

		void Start()
		{
			var loadRequest = FindAnyObjectByType<LoadRequest>();
			if (loadRequest)
			{
				var reader = loadRequest.Consume();
				var restoreWorld = RestoreWorld(reader);
				restoreWorld.ContinueWith(() => reader.Close()).Forget(Debug.LogException);
			}
			else
			{
				StartWorld().Forget(Debug.LogException);
			}
		}

		async void OnDestroy()
		{
			await World.Destroy();
			_initTask = default;
		}

		void Update()
		{
			if (_initTask.Status != UniTaskStatus.Succeeded)
			{
				return;
			}
			World.Update();
		}

		public UniTask StartWorld()
		{
			try
			{
				World     = CreateWorld();
				_initTask = World.Initialize();
				return _initTask;
			}
			catch (Exception e)
			{
				Debug.LogException(e, this);
				throw;
			}
		}
		
		public UniTask RestoreWorld(BinaryReader reader)
		{
			try
			{
				World     = CreateWorld();
				_initTask = World.Restore(reader);
				return _initTask;
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
	}
}
