﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using KVD.ECS.Core.Systems;
using KVD.Utils.DataStructures;
using Unity.Collections;
#if SYSTEM_PROFILER_MARKERS
using KVD.Utils.Extensions;
#endif
using UnityEditor;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class WorldDebugger : EditorWindow
	{
		static readonly BindingFlags FieldInfoFlag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic;
		static readonly FieldInfo SystemsField = typeof(World).GetField("systems", FieldInfoFlag)!;
		static readonly FieldInfo ComponentsFieldInfo = typeof(World).GetField("_componentsStorages", FieldInfoFlag)!;
		static readonly FieldInfo SingletonsFieldInfo = typeof(ComponentsStorage).GetField("_singletons", FieldInfoFlag)!;
		static readonly FieldInfo BucketsFieldInfo = typeof(SingletonComponentsStorage).GetField("_buckets", FieldInfoFlag)!;

		World? _world;
		World? World => _world ??= FindAnyObjectByType<WorldWrapper>()?.World;

		Vector2 _storagesScroll;
		Vector2 _systemsScroll;
		string _systemsFilter = string.Empty;
		readonly OnDemandDictionary<ComponentsStorageKey, bool> _foldoutByKey = new();
		readonly Dictionary<ISystem, SystemWrapper> _wrapperBySystem = new();
		readonly Dictionary<ComponentsStorageKey, (int version, List<(Entity entity, string displayData)> entities)> _entitiesCache = new(ComponentsStorageKey.ComponentStorageKeyComparer);
		UnsafeBitmask _uniqueEntitiesCollector = new(1, Allocator.Persistent);

		bool _systemsExpanded;
		bool _storageExpanded;
		
		#region Tables definitions
		readonly TableView<SystemWrapper> _systemsTableView = new(new[]
		{
			new TableColumn<SystemWrapper>("Name", w => w.DisplayName, 7f/10),
			//new TableColumn<SystemWrapper>("Entities", w => (w.ComponentsViews?.FirstOrDefault()?.Size ?? 0).ToString(), 1f/10),
#if SYSTEM_PROFILER_MARKERS
			new TableColumn<SystemWrapper>("Time", w => $"{w.Recorder.GetRecorderAverageTime():f4} ms", 2f/10),
#endif
		});

		readonly TableView<ComponentList> _sparseListTableView = new(new[]
		{
			new TableColumn<ComponentList>("Name", ComponentName, 2f/7),
			new TableColumn<ComponentList>("Entities", l => $"{l.length}/{l.capacity}", 1f/7),
			new TableColumn<ComponentList>("Size (single/in-use/alloc)", l =>
			{
				var size = l.typeInfo.valueSize;
				return $"{size}B/{size*l.length/1024f:f2}kB/{size*l.capacity/1024f:f2}kB";
			}, 3f/7),
			new TableColumn<ComponentList>("Full list memory", l =>
			{
				var size = StorageWorldSizeUtils.FullListSize(l);
				return $"{size/1024f:f2}kB";
			}, 1f/7),
		});

		readonly TableView<SingletonComponentsStorage.IBucket> _singletonTableView = new(new[]
		{
			new TableColumn<SingletonComponentsStorage.IBucket>("Type", s => s.TargetType.Name, 1),
		});
		#endregion Tables definitions
	
		[MenuItem("KVD/ECS/World debugger")]
		static void Init()
		{
			var window = GetWindow<WorldDebugger>();
			window.titleContent = new("World debug");
			window.Show();
		}

		void OnEnable()
		{
			EditorApplication.playModeStateChanged -= PlayModeChanged;
			EditorApplication.playModeStateChanged += PlayModeChanged;
			
			if (EditorApplication.isPlaying)
			{
				PlayModeChanged(PlayModeStateChange.EnteredEditMode);
			}
		}

		void OnDisable()
		{
			EditorApplication.playModeStateChanged -= PlayModeChanged;
			Clean();
		}

		void PlayModeChanged(PlayModeStateChange change)
		{
			_world = null;
			if (change == PlayModeStateChange.EnteredPlayMode)
			{
				EditorApplication.update -= Repaint;
				EditorApplication.update += Repaint;
			}
			else
			{
				EditorApplication.update -= Repaint;
				Clean();
			}
		}

		void OnGUI()
		{
			if (!Application.isPlaying)
			{
				GUILayout.Label("Enter playmode in order to debug world configuration.", EditorStyles.boldLabel);
				return;
			}
			if (World == null)
			{
				GUILayout.Label("Cannot find World", EditorStyles.boldLabel);
				return;
			}
	
			
			DrawStorages();
			EditorGUILayout.Separator();
			DrawSystems();
		}

		void Clean()
		{
			foreach (var wrapper in _wrapperBySystem.Values)
			{
				wrapper.Dispose();
			}
			_wrapperBySystem.Clear();
			_entitiesCache.Clear();
		}
	
		#region Storages
		void DrawStorages()
		{
			var storagesDictionary  = (Dictionary<ComponentsStorageKey, ComponentsStorage>)ComponentsFieldInfo.GetValue(World);

			_storageExpanded = EditorGUILayout.Foldout(_storageExpanded, "Storages:");
			if (!_storageExpanded)
			{
				return;
			}

			_storagesScroll = EditorGUILayout.BeginScrollView(_storagesScroll, GUILayout.ExpandHeight(false));
			var fullSize = 0ul;
			foreach (var (key, storage) in storagesDictionary)
			{
				fullSize += DrawStorage(key, storage);
			}
			EditorGUILayout.LabelField($"Ful storages size: {fullSize/1024f/1024f:f2}MB");
			EditorGUILayout.EndScrollView();
		}

		ulong DrawStorage(ComponentsStorageKey key, ComponentsStorage storage)
		{
			var fold       = _foldoutByKey[key];
			_foldoutByKey[key] = EditorGUILayout.BeginFoldoutHeaderGroup(fold, $"Key: {ComponentsStorageKey.Name(key)}");
			var listsSize = 0ul;
			foreach (var list in storage.AllLists)
			{
				listsSize += StorageWorldSizeUtils.FullListSize(list);
			}
			
			if (!_foldoutByKey[key])
			{
				EditorGUI.EndFoldoutHeaderGroup();
				return listsSize;
			}
	
			var currentVersion = 0;
	
			GUILayout.Label($"Current entity: {storage.CurrentEntity}");
			GUILayout.Label($"Components:", EditorStyles.boldLabel);
			_sparseListTableView.Begin(position.width);
			_sparseListTableView.DrawHeader();
			foreach (var listPtr in storage.AllLists)
			{
				if (listPtr.IsCreated)
				{
					ref var list = ref listPtr.ToList();
					currentVersion += list.entitiesVersion;
					_sparseListTableView.DrawRow(list);
				}
			}
			_sparseListTableView.End();
			GUILayout.Space(12);
	
			var singletonsStorage = (SingletonComponentsStorage)SingletonsFieldInfo.GetValue(storage);
			var singletons        = (SingletonComponentsStorage.IBucket?[])BucketsFieldInfo.GetValue(singletonsStorage);
			_singletonTableView.DrawFull(singletons, position.width);
			GUILayout.Space(12);
			EditorGUI.EndFoldoutHeaderGroup();
			
			// === Entities
			return listsSize;
			if (!_entitiesCache.TryGetValue(key, out var entitiesData))
			{
				entitiesData        = (-1, new(128));
				_entitiesCache[key] = entitiesData;
			}
			if (entitiesData.version != currentVersion)
			{
				entitiesData.entities.Clear();
				_uniqueEntitiesCollector.Zero();
	
				entitiesData.version = currentVersion;
				foreach (var listPtr in storage.AllLists)
				{
					if (listPtr.IsCreated)
					{
						_uniqueEntitiesCollector.Union(listPtr.ToList().entitiesMask);
					}
				}
				foreach (var entity in _uniqueEntitiesCollector.EnumerateOnes())
				{
					entitiesData.entities.Add((new((int)entity), entity.ToString()));
				}
				entitiesData.entities.Sort((left, right)=>left.entity.index.CompareTo(right.entity.index));
				
				_entitiesCache[key] = entitiesData;
			}
			
			GUILayout.Label($"Entities:", EditorStyles.boldLabel);
			foreach (var entity in entitiesData.entities)
			{
				GUILayout.Label(entity.displayData);
			}
		}

		static string ComponentName(ComponentList list)
		{
			var componentType = list.typeInfo.typeHandle.Type;
			var componentName = componentType.Name;
			if (componentType.IsGenericType)
			{
				componentName += "<"+string.Join(", ", componentType.GetGenericArguments().Select(static t => t.Name))+">";
			}
			return componentName;
		}
		#endregion Storages
		
		#region Systems
		void DrawSystems()
		{
			var systems = (List<ISystem>)SystemsField.GetValue(World);
			
			_systemsExpanded = EditorGUILayout.Foldout(_systemsExpanded, "Systems:");
			
			if (!_systemsExpanded)
			{
				return;
			}

			_systemsFilter = EditorGUILayout.TextField("Search", _systemsFilter, EditorStyles.toolbarSearchField);
			_systemsScroll = EditorGUILayout.BeginScrollView(_systemsScroll, GUILayout.ExpandHeight(false));
			_systemsTableView.Begin(position.width);
			_systemsTableView.DrawHeader();
			foreach (var system in systems)
			{
				DrawSystem(system, 0);
			}
			_systemsTableView.End();
			
			EditorGUILayout.EndScrollView();
		}

		void DrawSystem(ISystem system, int depth)
		{
			if (!_wrapperBySystem.TryGetValue(system, out var wrapper))
			{
				wrapper                  = new(system, depth);
				_wrapperBySystem[system] = wrapper;
			}

			var oldColor = GUI.color;
			if (!string.IsNullOrWhiteSpace(_systemsFilter))
			{
				GUI.color = system.Name.Contains(_systemsFilter) ? Color.yellow : Color.gray;
			}
			

			if (system.InternalSystems.Count > 0)
			{
				_systemsTableView.DrawRow(wrapper, ref wrapper.expanded);
			}
			else
			{
				_systemsTableView.DrawRow(wrapper);
			}
			GUI.color = oldColor;

			if (!wrapper.expanded)
			{
				return;
			}

			EditorGUI.indentLevel++;
			for (var i = 0; i < system.InternalSystems.Count; i++)
			{
				var innerSystem = system.InternalSystems[i];
				DrawSystem(innerSystem, depth+1);
			}
			EditorGUI.indentLevel--;
		}
		#endregion Systems
	}
}
