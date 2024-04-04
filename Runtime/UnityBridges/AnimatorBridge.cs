using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using UnityEngine;

namespace KVD.ECS.UnityBridges
{
	public class AnimatorBridge : MonoBehaviour, IUnityBridge
	{
#nullable disable
		[SerializeField] Animator _animator;

		EcsToUnityLink _link;
		Entity _entity;
		ComponentListPtrSoft<SetAnimatorValue<int>> _setInts;
		ComponentListPtrSoft<SetAnimatorValue<float>> _setFloats;
		ComponentListPtrSoft<SetAnimatorValue<AnimatorBool>> _setBools;
		ComponentListPtrSoft<SetAnimatorValue<AnimatorTrigger>> _setTriggers;
#nullable enable

		public void Init()
		{
			_link   = GetComponentInParent<EcsToUnityLink>();
			_entity = _link.Entity;

			var storage = _link.Storage;
			_setInts     = storage.ListPtrSoft<SetAnimatorValue<int>>();
			_setFloats   = storage.ListPtrSoft<SetAnimatorValue<float>>();
			_setBools    = storage.ListPtrSoft<SetAnimatorValue<AnimatorBool>>();
			_setTriggers = storage.ListPtrSoft<SetAnimatorValue<AnimatorTrigger>>();
		}

		void Update()
		{
			if (_setInts.IsCreated)
			{
				ref var setInts = ref _setInts.ToList();
				var setInt = setInts.TryValue(_entity, out var has);
				if (has)
				{
					_animator.SetInteger(setInt.id, setInt.value);
					setInts.Remove(_entity);
				}
			}

			if (_setFloats.IsCreated)
			{
				ref var setFloats = ref _setFloats.ToList();
				var setFloat = setFloats.TryValue(_entity, out var has);
				if (has)
				{
					_animator.SetFloat(setFloat.id, setFloat.value);
					setFloats.Remove(_entity);
				}
			}

			if (_setBools.IsCreated)
			{
				ref var setBools = ref _setBools.ToList();
				var setBool = setBools.TryValue(_entity, out var has);
				if (has)
				{
					_animator.SetBool(setBool.id, setBool.value);
					setBools.Remove(_entity);
				}
			}

			if (_setTriggers.IsCreated)
			{
				ref var setTriggers = ref _setTriggers.ToList();
				var setTrigger = setTriggers.TryValue(_entity, out var has);
				if (has)
				{
					_animator.SetTrigger(setTrigger.id);
					setTriggers.Remove(_entity);
				}
			}
		}
	}
}
