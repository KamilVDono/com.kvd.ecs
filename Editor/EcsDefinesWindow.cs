using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace KVD.ECS.Editor
{
	public class EcsDefinesWindow : EditorWindow
	{
		private static readonly string[] Defines =
		{
			"ALLOCATORS_CHECKS", "ENTITIES_NAMES", "STORAGES_CHECKS", "LIST_PROFILER_MARKERS", "LIST_CHECKS",
			"SYSTEM_PROFILER_MARKERS",
		};

		private static readonly List<string> OtherDefines = new();
		private static bool[] _enables;

		[MenuItem("KVD/ECS/Defines")]
		private static void ShowWindow()
		{
			var window = GetWindow<EcsDefinesWindow>();
			window.Show();
		}

		private static void InitDefines()
		{
			_enables = new bool[Defines.Length];
			
			var target           = EditorUserBuildSettings.activeBuildTarget;
			var group            = BuildPipeline.GetBuildTargetGroup(target);
			var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
			PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var symbols);

			for (var i = 0; i < Defines.Length; i++)
			{
				var define = Defines[i];
				_enables[i] = Array.IndexOf(symbols, define) > -1;
			}
			
			OtherDefines.Clear();
			for (var i = 0; i < symbols.Length; i++)
			{
				if (Array.IndexOf(Defines, symbols[i]) < 0)
				{
					OtherDefines.Add(symbols[i]);
				}
			}
		}

		private void OnEnable()
		{
			InitDefines();
		}

		private void OnGUI()
		{
			for (var i = 0; i < Defines.Length; i++)
			{
				var define = Defines[i];
				_enables[i] = EditorGUILayout.Toggle(define, _enables[i]);
			}

			if (GUILayout.Button("Apply"))
			{
				var target           = EditorUserBuildSettings.activeBuildTarget;
				var group            = BuildPipeline.GetBuildTargetGroup(target);
				var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
				PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget,
					Defines.Where((_, i) => _enables[i]).Union(OtherDefines).ToArray());
			}
		}
	}
}
