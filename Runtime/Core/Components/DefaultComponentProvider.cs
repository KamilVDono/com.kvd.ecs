namespace KVD.ECS.Core.Components
{
	public struct DefaultComponentProvider<T> where T : unmanaged, IComponent
	{
		static T _default;

		public static ref T Default() => ref _default;
	}
}
