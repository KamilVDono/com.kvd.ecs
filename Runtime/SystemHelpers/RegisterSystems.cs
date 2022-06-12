using System;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.Utils.Attributes;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable

namespace KVD.ECS.SystemHelpers
{
	public class RegisterSystems : MonoBehaviour, IBootstrapable
	{
#nullable disable
		[SerializeField] private string _name;
		[SerializeReference, SubclassSelector,]
		private ISystem[] _systemReferences = Array.Empty<ISystem>();
		[SerializeField, SerializableInterface(typeof(ISystem)),]
		private MonoBehaviour[] _systemMonoBehaviourReferences = Array.Empty<MonoBehaviour>();
#nullable enable

		private SystemsGroup? _myGroup;

		public void OnDestroy()
		{
			if (_myGroup != null)
			{
				_myGroup!.Destroy().Forget(Debug.LogException);
			}
			_myGroup = null;
		}
		
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
			var systemName = string.IsNullOrWhiteSpace(_name) ? name : _name;
			_myGroup = new(systemName, MergeSystemsReferences());
			return world.RegisterSystem(_myGroup!);
		}

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
	}
}
