using System;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable

namespace KVD.ECS.SystemHelpers
{
	public class RegisterSystems : MonoBehaviour, IBootstrapable
	{
#nullable disable
		[SerializeField] string _name;
		[SerializeReference, SubclassSelector,]
		ISystem[] _systemReferences = Array.Empty<ISystem>();
#nullable enable

		SystemsGroup? _myGroup;

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

		UniTask Register(World world)
		{
			var systemName = string.IsNullOrWhiteSpace(_name) ? name : _name;
			_myGroup = new(systemName, _systemReferences);
			return world.RegisterSystem(_myGroup!);
		}
	}
}
