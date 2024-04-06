using KVD.ECS.Core;
using KVD.ECS.GeneralTests.Components;
using NUnit.Framework;

#nullable enable

namespace KVD.ECS.GeneralTests
{
	public class ComponentsStorageTests
	{
		ComponentsStorage _storage;

		[SetUp]
		public void Setup()
		{
			_storage = new();
		}

		[TearDown]
		public void TearDown()
		{
			_storage.Dispose();
		}
		
		[Test]
		public unsafe void List_MultipleOperations()
		{
			// Act&Assert
			var has = _storage.TryGetListPtr<Circle>(out _);
			Assert.IsFalse(has);
			var l1 = _storage.ListPtr<Circle>();
			var l2 = _storage.ListPtr<Circle>();
			has = _storage.TryGetListPtr<Circle>(out var l3);
			Assert.IsTrue(has);
			Assert.IsNotNull(l1);
			Assert.IsTrue(l1.ptr == l2.ptr);
			Assert.IsTrue(l1.ptr == l3.ptr);
		}
		
		#region IsAlive
		[Test]
		public void IsAlive_NotInserted_NotAlive()
		{
			// Arrange
			InitializeStandard();
			var nextEntity = _storage.NextEntity();
		
			// Assert
			Assert.IsFalse(_storage.IsAlive(nextEntity));
			Assert.IsFalse(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
		}
		
		[Test]
		public void IsAlive_Inserted_Alive()
		{
			// Arrange
			InitializeStandard();
			var nextEntity = _storage.Add<Acceleration, Circle>(new(), new());
		
			// Assert
			Assert.IsTrue(_storage.IsAlive(nextEntity));
			Assert.IsTrue(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
		}
		
		[Test]
		public void IsAlive_InsertedAndRemoved_NotAlive()
		{
			// Arrange
			InitializeStandard();
			var nextEntity = _storage.Add<Acceleration, Circle>(new(), new());
			_storage.RemoveEntity(nextEntity);
		
			// Assert
			Assert.IsFalse(_storage.IsAlive(nextEntity));
			Assert.IsFalse(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
		}
		
		[Test]
		public void IsAlive_MultipleOperations()
		{
			// Arrange
			InitializeStandard();
			var nextEntity = _storage.Add<Acceleration, Circle>(new(), new());
		
			// Act&Assert
			Assert.IsTrue(_storage.IsAlive(nextEntity));
			Assert.IsTrue(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
			
			_storage.Remove<Acceleration>(nextEntity);
			Assert.IsTrue(_storage.IsAlive(nextEntity));
			Assert.IsTrue(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
			
			_storage.Remove<Circle>(nextEntity);
			Assert.IsFalse(_storage.IsAlive(nextEntity));
			Assert.IsFalse(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
			
			_storage.Add<Circle>(nextEntity, new());
			Assert.IsTrue(_storage.IsAlive(nextEntity));
			Assert.IsTrue(_storage.IsAlive(0));
			Assert.IsFalse(_storage.IsAlive(1));
			Assert.IsFalse(_storage.IsAlive(50));
		}
		#endregion IsAlive
		// -- Singletons
		#region Singleton
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_AddSingleton(bool singleFrame)
		{
			// Act&Assert
			Assert.IsFalse(_storage.HasSingleton<Circle>());
			_storage.Singleton<Circle>(new(), singleFrame);
			Assert.IsTrue(_storage.HasSingleton<Circle>());
		
		}
		// Get singleton
		[Test]
		public void Singleton_GetSingleton_NotPresent_Exception()
		{
			// Act&Assert
			Assert.Catch(() => _storage.Singleton<Circle>());
		
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			var singletonCircle = _storage.Singleton<Circle>();
			// Assert
			Assert.AreEqual(circle, singletonCircle);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetRefSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			ref var singletonCircle1 = ref _storage.Singleton<Circle>();
			ref var singletonCircle2 = ref _storage.Singleton<Circle>();
			singletonCircle1.id = 69;
			var idAfterChange = _storage.Singleton<Circle>().id;
			// Assert
			Assert.AreEqual(singletonCircle1, singletonCircle2);
			Assert.AreEqual(singletonCircle1.id, idAfterChange);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetOrDefaultSingleton_NotPresent(bool singleFrame)
		{
			// Arrange&Act
			var circle = _storage.SingletonOrDefault<Circle>();
			// Assert
			Assert.AreEqual(default(Circle), circle);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetOrDefaultSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			var singletonCircle = _storage.SingletonOrDefault<Circle>();
			// Assert
			Assert.AreEqual(circle, singletonCircle);
		}
		
		[Test]
		public void Singleton_GetOrNewSingleton_NotPresent()
		{
			// Arrange&Act
			var singletonCircle = _storage.SingletonOrNew<Circle>();
			// Assert
			Assert.AreEqual(default(Circle), singletonCircle);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetOrNewSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			var singletonCircle = _storage.SingletonOrNew<Circle>();
			// Assert
			Assert.AreEqual(circle, singletonCircle);
		}
		[Test]
		public void Singleton_GetOrNewRefSingleton_NotPresent()
		{
			// Arrange&Act
			ref var singletonCircle1 = ref _storage.SingletonOrNew<Circle>();
			ref var singletonCircle2 = ref _storage.SingletonOrNew<Circle>();
			singletonCircle1.id = 69;
			var idAfterChange = _storage.Singleton<Circle>().id;
			// Assert
			Assert.AreEqual(singletonCircle1, singletonCircle2);
			Assert.AreEqual(singletonCircle1.id, idAfterChange);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_GetOrNewRefSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			ref var singletonCircle1 = ref _storage.SingletonOrNew<Circle>();
			ref var singletonCircle2 = ref _storage.SingletonOrNew<Circle>();
			singletonCircle1.id = 69;
			var idAfterChange = _storage.SingletonOrNew<Circle>().id;
			// Assert
			Assert.AreEqual(singletonCircle1, singletonCircle2);
			Assert.AreEqual(singletonCircle1.id, idAfterChange);
		}
		
		[Test]
		public void Singleton_TrySingleton_NotPresent()
		{
			// Arrange&Act
			var _ = _storage.TrySingleton<Circle>(out var has);
			// Assert
			Assert.IsFalse(has);
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_TrySingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			var singletonCircle = _storage.TrySingleton<Circle>(out var has);
			// Assert
			Assert.IsTrue(has);
			Assert.AreEqual(circle, singletonCircle);
		}
		[Test]
		public void Singleton_TryRefSingleton_NotPresent()
		{
			// Arrange&Act
			ref var _ = ref _storage.TrySingleton<Circle>(out var has);
			// Assert
			Assert.IsFalse(has);
			Assert.IsFalse(_storage.HasSingleton<Circle>());
		}
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_TryRefSingleton_Present(bool singleFrame)
		{
			// Arrange
			var circle = new Circle { id = 5, };
			_storage.Singleton(circle, singleFrame);
			// Act
			ref var singletonCircle1 = ref _storage.TrySingleton<Circle>(out var has1);
			ref var singletonCircle2 = ref _storage.TrySingleton<Circle>(out var has2);
			singletonCircle1.id = 69;
			var idAfterChange = _storage.Singleton<Circle>().id;
			// Assert
			Assert.IsTrue(has1);
			Assert.IsTrue(has2);
			Assert.AreEqual(singletonCircle1, singletonCircle2);
			Assert.AreEqual(singletonCircle1.id, idAfterChange);
		}
		// Remove singleton
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_RemoveSingleton(bool singleFrame)
		{
			// Act
			_storage.Singleton<Circle>(new(), singleFrame);
			_storage.RemoveSingleton<Circle>();
			// Assert
			Assert.IsFalse(_storage.HasSingleton<Circle>());
		}
		// Clear singe frame
		[Test, TestCase(true), TestCase(false),]
		public void Singleton_ClearSingleFrame_SingleAdd(bool singleFrameAdd)
		{
			// Arrange
			var shouldPersist = !singleFrameAdd;
			_storage.Singleton<Circle>(new(), singleFrameAdd);
			// Act
			_storage.ClearSingleFrameEntities();
			// Assert
			Assert.AreEqual(shouldPersist, _storage.HasSingleton<Circle>());
		}
		[Test]
		public void Singleton_ClearSingleFrame_MultipleAdd(
			[Values(true, false)] bool firstFrame, [Values(true, false)] bool secondFrame)
		{
			// Arrange
			var shouldPersistFirst  = !firstFrame;
			_storage.Singleton<Circle>(new(), firstFrame);
			// Act
			_storage.ClearSingleFrameEntities();
			// Assert
			Assert.AreEqual(shouldPersistFirst, _storage.HasSingleton<Circle>());
			
			// Arrange
			var shouldPersistSecond = !secondFrame;
			_storage.Singleton<Circle>(new(), secondFrame);
			// Act
			_storage.ClearSingleFrameEntities();
			// Assert
			Assert.AreEqual(shouldPersistSecond, _storage.HasSingleton<Circle>());
		}
		[Test]
		public void Singleton_ClearSingleFrame_OverrideSingleFrame(
			[Values(true, false)] bool firstFrame, [Values(true, false)] bool secondFrame)
		{
			// Arrange
			_storage.Singleton<Circle>(new(), firstFrame);
			_storage.Singleton<Circle>(new(), secondFrame);
			// Act
			_storage.ClearSingleFrameEntities();
			// Assert
			Assert.AreEqual(!secondFrame, _storage.HasSingleton<Circle>());
		}
		#endregion Singleton
		
		#region Helpers
		private void InitializeStandard()
		{
			_storage.ListPtr<Acceleration>();
			_storage.ListPtr<Borders>();
			_storage.ListPtr<Circle>();
		}
		#endregion Helpers
	}
}
