namespace KVD.ECS.Systems
{
	public abstract class ComponentsSystemBase<TWorld> : SystemBase where TWorld : World
	{
		public new TWorld World => (TWorld)base.World;
	}
}
