using System;
using System.Diagnostics;
using KVD.ECS.Benchmarks.Tests.Benchmarks.Components;
using KVD.ECS.ComponentHelpers;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.Serializers;
using KVD.Utils.Extensions;
using Unity.IL2CPP.CompilerServices.Unity.Il2Cpp;
using Unity.Profiling;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace KVD.ECS.Benchmarks.Tests.Benchmarks
{
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class ComponentsStorageComponent : MonoBehaviour
	{
		private const int EntitiesCount = 0;
		private const int Iterations = 1;

#nullable disable
		private ZipAndUpdate_StaticUpdate _static;
		private ZipAndUpdate_StaticUpdateGet _staticGet;
		private ZipAndUpdate_DynamicUpdate _dynamic;
		private ZipAndUpdate_DynamicUpdateGet _dynamicGet;

		private ProfilerRecorder[] _recorders;
		private string[] _names;
		private string _lastInsertTime = string.Empty;
#nullable enable

		public void Start()
		{
			// Just initialize to not pollute results
			SerializersLibrary.Serializer<PrefabWrapper>();
			
			_static     = new(EntitiesCount);
			_staticGet  = new(EntitiesCount);
			_dynamic    = new(EntitiesCount);
			_dynamicGet = new(EntitiesCount);

			_recorders = new[]
			{
				ProfilerRecorder.StartNew(ZipAndUpdate_StaticUpdate.ZipAndUpdateMarker, 50),
				ProfilerRecorder.StartNew(ZipAndUpdate_StaticUpdateGet.ZipAndUpdateMarker, 50),
				ProfilerRecorder.StartNew(ZipAndUpdate_DynamicUpdate.ZipAndUpdateMarker, 50),
				ProfilerRecorder.StartNew(ZipAndUpdate_DynamicUpdateGet.ZipAndUpdateMarker, 50),
#if DEBUG
				ProfilerRecorder.StartNew(SparseListConstants.AddMarker, 10),
				ProfilerRecorder.StartNew(SparseListConstants.EnsureSizeMarker, 10),
				ProfilerRecorder.StartNew(SparseListConstants.RemoveMarker, 10),
#endif
			};


			_names = new[]
			{
				"StaticUpdate",
				"StaticUpdate Get",
				"DynamicUpdate",
				"DynamicUpdate Get",
#if DEBUG
				"SparseList.Add",
				"SparseList.EnsureSize",
				"SparseList.Remove",
#endif
			};
			GC.Collect();
		}

		private void Update()
		{
			_static.Update(Iterations);
			_staticGet.Update(Iterations);
			_dynamic.Update(Iterations);
			_dynamicGet.Update(Iterations);
		}

		private void OnDestroy()
		{
			foreach (var profilerRecorder in _recorders)
			{
				profilerRecorder.Dispose();
			}
			
			_static.Destroy();
			_staticGet.Destroy();
			_dynamic.Destroy();
			_dynamicGet.Destroy();
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			foreach (var recorderName in _names)
			{
				var text = $"{recorderName}";
				GUILayout.Label(text);
			}
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			for (var i = 0; i < _recorders.Length; i++)
			{
				var text = $"{_recorders[i].GetRecorderAverageTime():f4} ms";
				GUILayout.Label(text);
			}
			GUILayout.Label($"Uses {UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong()/1024/1024} MB");
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			GUILayout.Label($"Entities count: {_dynamic.Count}");
			GUILayout.Label(_lastInsertTime);
			if (GUILayout.Button("Add 100"))
			{
				Add(100);
			}
			if (GUILayout.Button("Add 1_000"))
			{
				Add(1000);
			}
			if (GUILayout.Button("Add 5_000"))
			{
				Add(5000);
			}
			if (GUILayout.Button("Add 10_000"))
			{
				Add(10000);
			}
			if (GUILayout.Button("Add 50_000"))
			{
				Add(50000);
			}
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		}
		
		private void Add(int count)
		{
			Stopwatch sw = new();
			sw.Start();
			_static.Add(count);
			_staticGet.Add(count);
			_dynamic.Add(count);
			_dynamicGet.Add(count);
			sw.Stop();
			_lastInsertTime = $"Last insert time: {sw.Elapsed.TotalMilliseconds:f6} ms";
			GC.Collect();
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class ZipAndUpdate_StaticUpdate
	{
		public static readonly ProfilerMarker ZipAndUpdateMarker = new(nameof(ZipAndUpdate_StaticUpdate));

		private readonly ComponentsView _view;
		private readonly ComponentsStorage _components;
		private readonly SparseList<Radius> _radii;
		private readonly SparseList<Position> _positions;
		private readonly SparseList<Acceleration> _accelerations;

		public int Count => _components.CurrentEntity+1;

		public ZipAndUpdate_StaticUpdate(int entitiesCount)
		{
			_components    = new();
			_radii         = _components.List<Radius>(entitiesCount);
			_positions     = _components.List<Position>(entitiesCount);
			_accelerations = _components.List<Acceleration>(entitiesCount);
			
			Add(entitiesCount);

			_view = new(
				ViewDescriptor.New<Radius, Position, Acceleration>(_components)
				);
		}

		public void Update(int iterations)
		{
			using var marker = ZipAndUpdateMarker.Auto();
			for (var i = 0; i < iterations; ++i)
			{
				foreach (var entity in _view)
				{
					var     radius       = _radii.Value(entity);
					ref var position     = ref _positions.Value(entity);
					var     acceleration = _accelerations.Value(entity);

					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;
				
					position.x += radius.r;
				}
			}
		}

		public void Destroy()
		{
			_components.Destroy();
		}
		
		public void Add(int entitiesCount)
		{
			var lastEntity = new Entity(_components.CurrentEntity+entitiesCount);
			_radii.EnsureCapacity(entitiesCount + _radii.Length, lastEntity);
			_positions.EnsureCapacity(entitiesCount/2 + _positions.Length, lastEntity);
			_accelerations.EnsureCapacity(entitiesCount/3 + _accelerations.Length, lastEntity);
			
			for (var i = 0; i < entitiesCount; ++i)
			{
				var entity = _components.NextEntity();
				_radii.Add(entity, new() { r = i%5, });

				if (i%2 == 0)
				{
					_positions.Add(entity, new() { x = i%50, });
				}

				if (i%3 == 0)
				{
					_accelerations.Add(entity, new());
				}
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class ZipAndUpdate_StaticUpdateGet
	{
		public static readonly ProfilerMarker ZipAndUpdateMarker = new(nameof(ZipAndUpdate_StaticUpdateGet));

		private readonly ComponentsView<Radius, Position, Acceleration> _view;
		private readonly ComponentsStorage _components;
		private readonly SparseList<Radius> _radii;
		private readonly SparseList<Position> _positions;
		private readonly SparseList<Acceleration> _accelerations;

		public int Count => _components.CurrentEntity+1;

		public ZipAndUpdate_StaticUpdateGet(int entitiesCount)
		{
			_components    = new();
			_radii         = _components.List<Radius>(entitiesCount);
			_positions     = _components.List<Position>(entitiesCount);
			_accelerations = _components.List<Acceleration>(entitiesCount);
			
			Add(entitiesCount);

			_view = new(_components);
		}

		public void Update(int iterations)
		{
			using var marker = ZipAndUpdateMarker.Auto();
			for (var i = 0; i < iterations; ++i)
			{
				foreach (var iter in _view)
				{
					var     radius       = iter.Get0();
					ref var position     = ref iter.Get1();
					var     acceleration = iter.Get2();

					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;
				
					position.x += radius.r;
				}
			}
		}

		public void Destroy()
		{
			_components.Destroy();
		}
		
		public void Add(int entitiesCount)
		{
			var lastEntity = new Entity(_components.CurrentEntity+entitiesCount);
			_radii.EnsureCapacity(entitiesCount + _radii.Length, lastEntity);
			_positions.EnsureCapacity(entitiesCount/2 + _positions.Length, lastEntity);
			_accelerations.EnsureCapacity(entitiesCount/3 + _accelerations.Length, lastEntity);
			
			for (var i = 0; i < entitiesCount; ++i)
			{
				var entity = _components.NextEntity();
				_radii.Add(entity, new() { r = i%5, });

				if (i%2 == 0)
				{
					_positions.Add(entity, new() { x = i%50, });
				}

				if (i%3 == 0)
				{
					_accelerations.Add(entity, new());
				}
			}
		}
	}

	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class ZipAndUpdate_DynamicUpdate
	{
		public static readonly ProfilerMarker ZipAndUpdateMarker = new(nameof(ZipAndUpdate_DynamicUpdate));

		private readonly ComponentsView _view1;
		private readonly ComponentsView _view2;
		private readonly ComponentsStorage _components;
		private readonly SparseList<Radius> _radii;
		private readonly SparseList<Position> _positions;
		private readonly SparseList<Acceleration> _accelerations;
		
		public int Count => _components.CurrentEntity+1;

		public ZipAndUpdate_DynamicUpdate(int entitiesCount)
		{
			_components    = new();
			_radii         = _components.List<Radius>(entitiesCount);
			_positions     = _components.List<Position>(entitiesCount);
			_accelerations = _components.List<Acceleration>(entitiesCount);
			
			Add(entitiesCount);

			_view1 = new(
				ViewDescriptor.New<Radius, Position, Acceleration>(_components)
				);
			_view2 = new(
				ViewDescriptor.New<Position, Acceleration>(_components, new[] { typeof(Radius), })
				);
		}

		public void Update(int iterations)
		{
			using var marker = ZipAndUpdateMarker.Auto();
			for (var i = 0; i < iterations; ++i)
			{
				foreach (var entity in _view1)
				{
					var     radius       = _radii.Value(entity);
					ref var position     = ref _positions.Value(entity);
					var     acceleration = _accelerations.Value(entity);

					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;

					position.x += radius.r;

					if (position.x > 300)
					{
						_radii.Remove(entity);
					}
				}
				
				var r = 0;
				foreach (var entity in _view2)
				{
					ref var position = ref _positions.Value(entity);
					position.x = -position.x-iterations*5;

					_radii.Add(entity, new() { r = r++, });
				}
			}
		}

		public void Destroy()
		{
			_components.Destroy();
		}
		
		public void Add(int entitiesCount)
		{
			var lastEntity = new Entity(_components.CurrentEntity+entitiesCount);
			_radii.EnsureCapacity(entitiesCount + _radii.Length, lastEntity);
			_positions.EnsureCapacity(entitiesCount/2 + _positions.Length, lastEntity);
			_accelerations.EnsureCapacity(entitiesCount/3 + _accelerations.Length, lastEntity);
			
			for (var i = 0; i < entitiesCount; ++i)
			{
				var entity = _components.NextEntity();
				_radii.Add(entity, new() { r = i%5, });

				if (i%2 == 0)
				{
					_positions.Add(entity, new() { x = i%50, });
				}

				if (i%3 == 0)
				{
					_accelerations.Add(entity, new());
				}
			}
		}
	}
	
	[Il2CppSetOption(Option.NullChecks, false), Il2CppSetOption(Option.ArrayBoundsChecks, false),]
	public class ZipAndUpdate_DynamicUpdateGet
	{
		public static readonly ProfilerMarker ZipAndUpdateMarker = new(nameof(ZipAndUpdate_DynamicUpdateGet));

		private readonly ComponentsView<Radius, Position, Acceleration> _view1;
		private readonly ComponentsView<Position> _view2;
		private readonly ComponentsStorage _components;
		private readonly SparseList<Radius> _radii;
		private readonly SparseList<Position> _positions;
		private readonly SparseList<Acceleration> _accelerations;
		
		public int Count => _components.CurrentEntity+1;

		public ZipAndUpdate_DynamicUpdateGet(int entitiesCount)
		{
			_components    = new();
			_radii         = _components.List<Radius>(entitiesCount);
			_positions     = _components.List<Position>(entitiesCount);
			_accelerations = _components.List<Acceleration>(entitiesCount);
			
			Add(entitiesCount);

			_view1 = new(_components);
			_view2 = new(_components, new[] { typeof(Acceleration), }, new[] { typeof(Radius), });
		}

		public void Update(int iterations)
		{
			using var marker = ZipAndUpdateMarker.Auto();
			for (var i = 0; i < iterations; ++i)
			{
				foreach (var iter in _view1)
				{
					var     radius       = iter.Get0();
					ref var position     = ref iter.Get1();
					var     acceleration = iter.Get2();

					position.x += acceleration.x;
					position.y += acceleration.y;
					position.z += acceleration.z;

					position.x += radius.r;

					if (position.x > 300)
					{
						_radii.Remove(iter.entity);
					}
				}
				
				var r = 0;
				foreach (var iter in _view2)
				{
					ref var position = ref iter.Get0();
					position.x = -position.x-iterations*5;

					_radii.Add(iter.entity, new() { r = r++, });
				}
			}
		}

		public void Destroy()
		{
			_components.Destroy();
		}
		
		public void Add(int entitiesCount)
		{
			var lastEntity = new Entity(_components.CurrentEntity+entitiesCount);
			_radii.EnsureCapacity(entitiesCount + _radii.Length, lastEntity);
			_positions.EnsureCapacity(entitiesCount/2 + _positions.Length, lastEntity);
			_accelerations.EnsureCapacity(entitiesCount/3 + _accelerations.Length, lastEntity);
			
			for (var i = 0; i < entitiesCount; ++i)
			{
				var entity = _components.NextEntity();
				_radii.Add(entity, new() { r = i%5, });

				if (i%2 == 0)
				{
					_positions.Add(entity, new() { x = i%50, });
				}

				if (i%3 == 0)
				{
					_accelerations.Add(entity, new());
				}
			}
		}
	}
}
