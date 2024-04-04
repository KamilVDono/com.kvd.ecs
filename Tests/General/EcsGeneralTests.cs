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
		ComponentListPtr<Position> _positions;
		ComponentListPtr<Acceleration> _accelerations;
		
		protected override Task OnSetup()
		{
			_positions         = world.defaultStorage.ListPtr<Position>();
			_accelerations     = world.defaultStorage.ListPtr<Acceleration>();
			return base.OnSetup();
		}
		
		[UnityTest]
		public IEnumerator Systems_WorldUpdate_SystemsUpdate()
		{
			// Arrange
			yield return world.RegisterSystem(new MovementSystem()).ToCoroutine();
			
			var nextEntity = world.defaultStorage.NextEntity();
			_positions.AsList().Add(nextEntity, new());
			_accelerations.AsList().Add(nextEntity, new() { x = 1, y = 1, z = 1, });
			
			// Act & Assert
			for (var i = 0; i < 10; i++)
			{
				world.Update();
				Assert.AreEqual(i+1, _positions.AsList().Value(nextEntity).x);
				Assert.AreEqual(i+1, _positions.AsList().Value(nextEntity).y);
				Assert.AreEqual(i+1, _positions.AsList().Value(nextEntity).z);
			}
		}
		
		#region Helper data
		class MovementSystem : SystemBase
		{
			ComponentListPtr<Position> _positionComponents;
			ComponentListPtr<Acceleration> _accelerationComponents;
			ComponentsView _componentsView;
		
			protected override UniTask InitialSetup()
			{
				_positionComponents     = World.defaultStorage.ListPtr<Position>();
				_accelerationComponents = World.defaultStorage.ListPtr<Acceleration>();
				var builder = new ComponentsViewBuilder(World.defaultStorage, Allocator.Temp);
				_componentsView = builder.With<Position>().With<Acceleration>().Build(Allocator.Persistent, true);

				return base.InitialSetup();
			}
		
			protected override void Update()
			{
				foreach (var entity in _componentsView)
				{
					ref var position = ref _positionComponents.AsList().Value(entity);
					var acceleration = _accelerationComponents.AsList().Value(entity);
		
					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;
				}
			}
		}
		#endregion Helper data
	}
}
