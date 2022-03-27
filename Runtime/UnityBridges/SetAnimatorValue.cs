using KVD.ECS.Core.Components;

namespace KVD.ECS.UnityBridges
{
	public readonly struct SetAnimatorValue<T> : IComponent where T : unmanaged
	{
		public readonly int id;
		public readonly T value;
		
		public SetAnimatorValue(int id, T value)
		{
			this.id    = id;
			this.value = value;
		}

		public void Dispose() {}
	}
}
