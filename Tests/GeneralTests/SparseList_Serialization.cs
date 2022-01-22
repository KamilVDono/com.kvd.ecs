using System.IO;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.GeneralTests.Tests.GeneralTests.Components;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

#nullable disable

namespace KVD.ECS.GeneralTests.Tests.GeneralTests
{
	public class SparseListSerialization
	{
		private SparseList<Position> _positions;
		private SparseList<Circle> _circles;
		private SparseList<ComplexComponent> _complexComponents;

		private ISparseList _deserialized;
		
		[SetUp]
		public void OnSetup()
		{
			_positions         = new SparseList<Position>();
			_circles           = new SparseList<Circle>();
			_complexComponents = new SparseList<ComplexComponent>();
		}

		[TearDown]
		public void TearDown()
		{
			_positions.Destroy();
			_circles.Destroy();
			_complexComponents.Destroy();
			_deserialized.Destroy();
		}
		
		[Test]
		public void SimpleComponent_Empty()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			// Act
			_positions.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<Position>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_positions.Length, deserializedList!.Length);
		}
		
		[Test]
		public void SimpleComponent_Single()
		{
			// Arrange
			using var stream     = new MemoryStream();
			using var writer     = new BinaryWriter(stream);

			_positions.Add(new Entity(0), new Position { x = -50, y = 0, z = 69, });

			// Act
			_positions.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<Position>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_positions.Length, deserializedList!.Length);
			Assert.AreEqual(_positions.DenseArray[0].x, deserializedList.DenseArray[0].x);
			Assert.AreEqual(_positions.DenseArray[0].y, deserializedList.DenseArray[0].y);
			Assert.AreEqual(_positions.DenseArray[0].z, deserializedList.DenseArray[0].z);
		}
		
		[Test]
		public void SimpleComponent_Multiple()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			for (var i = 0; i < 3; i++)
			{
				_positions.Add(new Entity(i*2+0), new Position { x = -50, y = 0, z = 69, });
				_positions.Add(new Entity(i*2+1), new Position { x = 5f, y = 0.123f, z = 69.69f, });
			}
			

			// Act
			_positions.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<Position>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_positions.Length, deserializedList!.Length);
			Assert.AreEqual(_positions.DenseArray[0], deserializedList.DenseArray[0]);
			Assert.AreEqual(_positions.DenseArray[1], deserializedList.DenseArray[1]);
			Assert.AreEqual(_positions.DenseArray[2], deserializedList.DenseArray[2]);
			Assert.AreEqual(_positions.DenseArray[3], deserializedList.DenseArray[3]);
			Assert.AreEqual(_positions.DenseArray[4], deserializedList.DenseArray[4]);
			Assert.AreEqual(_positions.DenseArray[5], deserializedList.DenseArray[5]);
		}
		
		[Test]
		public void ComposedComponent_Empty()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			// Act
			_circles.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<Circle>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_circles.Length, deserializedList!.Length);
		}
		
		[Test]
		public void ComposedComponent_Single()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			_circles.Add(new Entity(0), new Circle
			{
				id = 5,
				position = new Position { x = 5, y = -15, z = 3, },
				radius = new Radius { r = 15, },
			});

			// Act
			_circles.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<Circle>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_circles.Length, deserializedList!.Length);
			Assert.AreEqual(_circles.DenseArray[0], deserializedList.DenseArray[0]);
		}
		
		[Test]
		public void ComplexComponent_Empty()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			// Act
			_complexComponents.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<ComplexComponent>.Deserialize(reader);
			_deserialized = deserializedList;

			// Assert
			Assert.AreEqual(_complexComponents.Length, deserializedList!.Length);
		}
		
		[Test]
		public void ComplexComponent_Single()
		{
			// Arrange
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream);

			var circle = new Circle
			{
				id       = 5,
				position = new Position { x = 5, y = -15, z = 3, },
				radius   = new Radius { r   = 15, },
			};
			var entities = new NativeArray<Entity>(5, Allocator.Persistent);
			entities[0] = new Entity(2);
			entities[3] = new Entity(5);
			entities[4] = new Entity(20);
			var floats = new NativeArray<float>(10, Allocator.Persistent);
			floats[1] = 1f;
			floats[5] = 0.123523f;
			var matrices = new NativeArray<float4x4>(1, Allocator.Persistent);
			matrices[0] = new float4x4(1, 2, 3, 4, 1, 2, 3, 4, 0.1f, 0.2f, 0.3f, 0.4f, 1.1f, 1.2f, 1.3f, 1.4f);
			var positions = new NativeArray<Position>(2, Allocator.Persistent);
			positions[0] = new Position();
			positions[1] = new Position { x = 1, y  = 1.1f, z = 5.12f, };
			_complexComponents.Add(new Entity(0), new ComplexComponent
			{
				circle = circle,
				transformation = float4x4.Ortho(5, 12, 0.01f, 150f),
				entities = entities,
				floats = floats,
				matrices = matrices,
				positions = positions,
			});

			// Act
			_complexComponents.Serialize(writer);

			stream.Flush();
			stream.Position = 0;
			using var reader = new BinaryReader(stream);

			var deserializedList = SparseList<ComplexComponent>.Deserialize(reader);
			_deserialized = deserializedList;
			
			var expected         = _complexComponents.DenseArray[0];
			var actual           = deserializedList!.DenseArray[0];

			// Assert
			Assert.AreEqual(_complexComponents.Length, deserializedList.Length);
			Assert.AreEqual(expected.circle, actual.circle);
			Assert.AreEqual(expected.transformation, actual.transformation);
			AssertHelper.AreEqual(expected.empty, actual.empty);
			AssertHelper.AreEqual(expected.entities, actual.entities);
			AssertHelper.AreEqual(expected.floats, actual.floats);
			AssertHelper.AreEqual(expected.matrices, actual.matrices);
			AssertHelper.AreEqual(expected.positions, actual.positions);
		}
	}
}
