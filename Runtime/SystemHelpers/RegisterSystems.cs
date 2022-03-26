using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.Utils.Attributes;
using UnityEngine;

#nullable enable

namespace KVD.ECS.SystemHelpers
{
	public class RegisterSystems : MonoBehaviour, IBootstrapable, ISystem
	{
		[SerializeReference, SubclassSelector,]
		private ISystem[] _systemReferences = Array.Empty<ISystem>();
		[SerializeField, SerializableInterface(typeof(ISystem)),]
		private MonoBehaviour[] _systemMonoBehaviourReferences = Array.Empty<MonoBehaviour>();

		private SystemsGroup? _myGroup;

		#region IBootstrapable
		UniTask IBootstrapable.Init(World world)
		{
			return Register(world);
		}
		
		public UniTask Restore(World world)
		{
			return Register(world);
		}
		
		private UniTask Register(World world)
		{
			CheckState();
			_myGroup = new(name, MergeSystemsReferences());
			return world.RegisterSystem(_myGroup!);
		}
		#endregion IBootstrapable
		
		#region ISystem
		public World World => _myGroup!.World;
		public string Name => _myGroup!.Name;
		public IReadOnlyList<ISystem> InternalSystems => _myGroup!.InternalSystems;
		
		UniTask ISystem.Init(World world)
		{
			CheckState();
			_myGroup = new(name, MergeSystemsReferences());
			return _myGroup.Init(world);
		}
		
		UniTask ISystem.Restore(World world)
		{
			CheckState();
			_myGroup = new(name, MergeSystemsReferences());
			return _myGroup.Restore(world);
		}
		
		public void DoUpdate()
		{
			_myGroup!.DoUpdate();
		}
		
		public UniTask Destroy()
		{
			return _myGroup!.Destroy();
		}
		#endregion ISystem

		private ISystem[] MergeSystemsReferences()
		{
			var systems = new ISystem[_systemReferences.Length + _systemMonoBehaviourReferences.Length];
			Array.Copy(_systemReferences, 0, systems, 0, _systemReferences.Length);
			for (var i = 0; i < _systemMonoBehaviourReferences.Length; i++)
			{
				systems[i+_systemReferences.Length] = (ISystem)_systemMonoBehaviourReferences[i];
			}
			return systems;
		}
		
		[Conditional("DEBUG")]
		private void CheckState()
		{
			if (_myGroup != null)
			{
				throw new ApplicationException($"RegisterSystems [{name}] acts as IBootstrapable and ISystem the same time ");
			}
		}
	}
}
