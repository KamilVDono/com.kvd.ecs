using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine.TestTools;

#nullable disable

namespace KVD.ECS.GeneralTests
{
	public class EcsGeneralTests : EcsTestsBase
	{
		private ComponentList<Position> _positions;
		private ComponentList<Acceleration> _accelerations;
		
		protected override Task OnSetup()
		{
			_positions         = world.defaultStorage.List<Position>();
			_accelerations     = world.defaultStorage.List<Acceleration>();
			return base.OnSetup();
		}
		
		[UnityTest]
		public IEnumerator Systems_WorldUpdate_SystemsUpdate()
		{
			// Arrange
			yield return world.RegisterSystem(new MovementSystem()).ToCoroutine();
			
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.Add(nextEntity, new());
			_accelerations.Add(nextEntity, new() { x = 1, y = 1, z = 1, });
			
			// Act & Assert
			for (var i = 0; i < 10; i++)
			{
				world.Update();
				Assert.AreEqual(i+1, _positions.Value(nextEntity).x);
				Assert.AreEqual(i+1, _positions.Value(nextEntity).y);
				Assert.AreEqual(i+1, _positions.Value(nextEntity).z);
			}
		}
		
		#region Helper data
		private class MovementSystem : SystemBase
		{
			private ComponentList<Position> _positionComponents;
			private ComponentList<Acceleration> _accelerationComponents;
			private ComponentsView _componentsView;
		
			protected override UniTask InitialSetup()
			{
				_positionComponents     = World.defaultStorage.List<Position>();
				_accelerationComponents = World.defaultStorage.List<Acceleration>();
				var builder = new ComponentsViewBuilder(World.defaultStorage, Allocator.Temp);
				_componentsView = builder.With<Position>().With<Acceleration>().Build(Allocator.Persistent, true);

				return base.InitialSetup();
			}
		
			protected override void Update()
			{
				foreach (var entity in _componentsView)
				{
					ref var position = ref _positionComponents.Value(entity);
					var acceleration = _accelerationComponents.Value(entity);
		
					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;
				}
			}
		}
		#endregion Helper data
	}
}
