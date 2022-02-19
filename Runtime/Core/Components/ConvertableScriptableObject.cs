using KVD.ECS.Core.Entities;
using UnityEngine;

namespace KVD.ECS.Core.Components
{
	public abstract class ConvertableScriptableObject : ScriptableObject
	{
		public abstract Entity Convert(Entity entity, World world, ComponentsStorage targetStorage);
	}
}
