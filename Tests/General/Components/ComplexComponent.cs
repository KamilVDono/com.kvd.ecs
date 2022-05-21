using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using Unity.Collections;
using Unity.Mathematics;

namespace KVD.ECS.GeneralTests.Components
{
	public struct ComplexComponent : IComponent
	{
		public Circle circle;
		public float4x4 transformation;
		public RentedArray<Entity> entities;
		public RentedArray<Position> positions;
		public RentedArray<float> floats;
		public RentedArray<float4x4> matrices;
		public RentedArray<Circle> empty;
		
		public void Dispose()
		{
			entities.Dispose();
			positions.Dispose();
			floats.Dispose();
			matrices.Dispose();
			empty.Dispose();
		}
	}
}
