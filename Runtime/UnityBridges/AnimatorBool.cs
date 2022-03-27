using System;

namespace KVD.ECS.UnityBridges
{
	[Serializable]
	public readonly struct AnimatorBool
	{
		public readonly byte value;
		
		public bool Bool => value != 0;

		public AnimatorBool(byte value)
		{
			this.value = value;
		}
		
		public AnimatorBool(bool value)
		{
			this.value = (byte)(value ? 1 : 0);
		}

		public static implicit operator bool(AnimatorBool val) => val.Bool;
		public static implicit operator AnimatorBool(bool val) => new(val);
	}
}
