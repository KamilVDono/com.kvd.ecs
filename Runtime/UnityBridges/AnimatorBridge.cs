using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public class AnimatorBridge : MonoBehaviour, IUnityBridge
	{
#nullable disable
		[SerializeField] private Animator _animator;

		private EcsToUnityLink _link;
		private Entity _entity;
		private ComponentList<SetAnimatorValue<int>> _setInts;
		private ComponentList<SetAnimatorValue<float>> _setFloats;
		private ComponentList<SetAnimatorValue<AnimatorBool>> _setBools;
		private ComponentList<SetAnimatorValue<AnimatorTrigger>> _setTriggers;
#nullable enable

		public void Init()
		{
			_link   = GetComponentInParent<EcsToUnityLink>();
			_entity = _link.Entity;

			var storage = _link.Storage;
			_setInts     = storage.List<SetAnimatorValue<int>>();
			_setFloats   = storage.List<SetAnimatorValue<float>>();
			_setBools    = storage.List<SetAnimatorValue<AnimatorBool>>();
			_setTriggers = storage.List<SetAnimatorValue<AnimatorTrigger>>();
		}

		private void Update()
		{
			var setInt = _setInts.TryValue(_entity, out var has);
			if (has)
			{
				_animator.SetInteger(setInt.id, setInt.value);
				_setInts.Remove(_entity);
			}
			
			var setFloat = _setFloats.TryValue(_entity, out has);
			if (has)
			{
				_animator.SetFloat(setFloat.id, setFloat.value);
				_setFloats.Remove(_entity);
			}
			
			var setBool = _setBools.TryValue(_entity, out has);
			if (has)
			{
				_animator.SetBool(setBool.id, setBool.value);
				_setBools.Remove(_entity);
			}
			
			var setTrigger = _setTriggers.TryValue(_entity, out has);
			if (has)
			{
				_animator.SetTrigger(setTrigger.id);
				_setTriggers.Remove(_entity);
			}
		}
	}
}
