using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using KVD.ECS.Core;
using KVD.ECS.Core.Entities;
using KVD.ECS.Core.Helpers;
using KVD.ECS.Core.Systems;
using KVD.Utils.DataStructures;
using KVD.Utils.Extensions;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class WorldDebugger : EditorWindow
	{
		private static readonly BindingFlags FieldInfoFlag = BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic;
		private static readonly FieldInfo SystemsField = typeof(World).GetField("systems", FieldInfoFlag)!;
		private static readonly FieldInfo ComponentsFieldInfo = typeof(World).GetField("_componentsStorages", FieldInfoFlag)!;
		private static readonly FieldInfo SingletonsFieldInfo = typeof(ComponentsStorage).GetField("_singletons", FieldInfoFlag)!;
		private static readonly FieldInfo BucketsFieldInfo = typeof(SingletonComponentsStorage).GetField("_buckets", FieldInfoFlag)!;

		private World? _world;
		private World? World => _world ??= FindObjectOfType<WorldWrapper>()?.World;

		private Vector2 _storagesScroll;
		private Vector2 _systemsScroll;
		private readonly OnDemandDictionary<ComponentsStorageKey, bool> _foldoutByKey = new();
		private readonly Dictionary<ISystem, SystemWrapper> _wrapperBySystem = new();
		private readonly Dictionary<ComponentsStorageKey, (int version, List<(Entity entity, string displayData)> entities)> _entitiesCache = new(ComponentsStorageKey.ComponentStorageKeyComparer);
		private readonly BigBitmask _uniqueEntitiesCollector = new();
		#region Tables definitions
		private readonly TableView<SystemWrapper> _systemsTableView = new(new[]
		{
			new TableColumn<SystemWrapper>("Name", w => w.DisplayName, 2f/5),
			new TableColumn<SystemWrapper>("Entities", w => (w.ComponentsViews?.FirstOrDefault()?.Size ?? 0).ToString(), 1f/5),
			new TableColumn<SystemWrapper>("Time", w => $"{w.Recorder.GetRecorderAverageTime():f4} ms", 2f/5),
		});
		
		private readonly TableView<IComponentList> _sparseListTableView = new(new[]
		{
			new TableColumn<IComponentList>("Name", ComponentName, 2f/6),
			new TableColumn<IComponentList>("Entities", l => $"{l.Length}/{l.Capacity}", 1f/6),
			new TableColumn<IComponentList>("Size (single/in-use/alloc)", l =>
			{
				var size = ComponentSize(l);
				return $"{size}B/{size*l.Length/1024}kB/{size*l.Capacity/1024}kB";
			}, 3f/6),
		});
		
		private readonly TableView<SingletonComponentsStorage.IBucket> _singletonTableView = new(new[]
		{
			new TableColumn<SingletonComponentsStorage.IBucket>("Type", s => s.TargetType.Name, 1),
		});
		#endregion Tables definitions

		[MenuItem("KVD/ECS/World debugger")]
		private static void Init()
		{
			var window = GetWindow<WorldDebugger>();
			window.titleContent = new("World debug");
			window.Show();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged -= PlayModeChanged;
			EditorApplication.playModeStateChanged += PlayModeChanged;
			
			if (EditorApplication.isPlaying)
			{
				PlayModeChanged(PlayModeStateChange.EnteredEditMode);
			}
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= PlayModeChanged;
			Clean();
		}

		private void PlayModeChanged(PlayModeStateChange change)
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

		private void OnGUI()
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

			_storagesScroll = EditorGUILayout.BeginScrollView(_storagesScroll, GUILayout.Height(position.height / 3 * 2));
			DrawStorages();
			EditorGUILayout.EndScrollView();
			
			_systemsScroll = EditorGUILayout.BeginScrollView(_systemsScroll, GUILayout.Height(position.height / 3));
			DrawSystems();
			EditorGUILayout.EndScrollView();
		}

		private void Clean()
		{
			foreach (var wrapper in _wrapperBySystem.Values)
			{
				wrapper.Dispose();
			}
			_wrapperBySystem.Clear();
			_entitiesCache.Clear();
		}

		#region Storages
		private void DrawStorages()
		{
			var storagesDictionary  = (Dictionary<ComponentsStorageKey, ComponentsStorage>)ComponentsFieldInfo.GetValue(World);
			
			GUILayout.Label("Storages:", EditorStyles.boldLabel);
			foreach (var (key, storage) in storagesDictionary)
			{
				DrawStorage(key, storage);
			}
		}
		
		private void DrawStorage(ComponentsStorageKey key, ComponentsStorage storage)
		{
			var fold       = _foldoutByKey[key];
			_foldoutByKey[key] = EditorGUILayout.BeginFoldoutHeaderGroup(fold, $"Key: {ComponentsStorageKey.Name(key)}");
			if (!_foldoutByKey[key])
			{
				EditorGUI.EndFoldoutHeaderGroup();
				return;
			}

			var currentVersion = 0;

			GUILayout.Label($"Current entity: {storage.CurrentEntity}");
			GUILayout.Label($"Components:", EditorStyles.boldLabel);
			_sparseListTableView.Begin(position.width);
			_sparseListTableView.DrawHeader();
			foreach (var list in storage.AllLists)
			{
				currentVersion += list.EntitiesVersion;
				_sparseListTableView.DrawRow(list);
			}
			_sparseListTableView.End();
			GUILayout.Space(12);

			var singletonsStorage = (SingletonComponentsStorage)SingletonsFieldInfo.GetValue(storage);
			var singletons        = (SingletonComponentsStorage.IBucket?[])BucketsFieldInfo.GetValue(singletonsStorage);
			_singletonTableView.DrawFull(singletons, position.width);
			GUILayout.Space(12);
			EditorGUI.EndFoldoutHeaderGroup();
			
			// === Entities
			return;
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
				foreach (var list in storage.AllLists)
				{
					_uniqueEntitiesCollector.Union(list.EntitiesMask);
				}
				foreach (var entity in _uniqueEntitiesCollector)
				{
					entitiesData.entities.Add((new(entity), entity.ToString()));
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
		
		private static string ComponentName(IComponentList list)
		{
			var componentType = list.GetType().GetGenericArguments()[0];
			var componentName = componentType.Name;
			if (componentType.IsGenericType)
			{
				componentName += "<"+string.Join(", ", componentType.GetGenericArguments().Select(t => t.Name))+">";
			}
			return componentName;
		}
		
		private static int ComponentSize(IComponentList list)
		{
			var componentType = list.GetType().GetGenericArguments()[0];
			return Marshal.SizeOf(componentType);
		}
		#endregion Storages
		
		#region Systems
		private void DrawSystems()
		{
			var systems      = (List<ISystem>)SystemsField.GetValue(World);
			
			GUILayout.Label("Systems:", EditorStyles.boldLabel);
			_systemsTableView.Begin(position.width);
			_systemsTableView.DrawHeader();
			foreach (var system in systems)
			{
				DrawSystem(system);
			}
			_systemsTableView.End();
		}
		
		private void DrawSystem(ISystem system)
		{
			if (!_wrapperBySystem.TryGetValue(system, out var wrapper))
			{
				wrapper                  = new(system);
				_wrapperBySystem[system] = wrapper;
			}

			_systemsTableView.DrawRow(wrapper);
			
			for (var i = 0; i < system.InternalSystems.Count; i++)
			{
				var innerSystem = system.InternalSystems[i];
				DrawSystem(innerSystem);
			}
		}
		#endregion Systems
	}
}
