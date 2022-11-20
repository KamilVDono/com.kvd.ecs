using KVD.ECS.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class WorldUIDebugger : EditorWindow
	{
		private const string HiddenClassName = "hidden";
#nullable disable
		[SerializeField] private VisualTreeAsset _visualTreeAsset;
#nullable restore
		
		private World? _world;
		private World? World => _world ??= FindObjectOfType<WorldWrapper>()?.World;
		
		[MenuItem("KVD/ECS/World UI debugger")]
		private static void Init()
		{
			var window = GetWindow<WorldUIDebugger>();
			window.titleContent = new("World debug");
			window.Show();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
		}

		private void CreateGUI()
		{
			_visualTreeAsset.CloneTree(rootVisualElement);
			RefreshState();
		}
		
		private void PlayModeStateChanged(PlayModeStateChange _)
		{
			RefreshState();
		}

		private void RefreshState()
		{
			var editModeContainer = rootVisualElement.Q<VisualElement>("EditMode");
			var playModeContainer = rootVisualElement.Q<VisualElement>("PlayMode");
			
			if (!Application.isPlaying)
			{
				playModeContainer.AddToClassList(HiddenClassName);
				editModeContainer.RemoveFromClassList(HiddenClassName);
				return;
			}
			
			editModeContainer.AddToClassList(HiddenClassName);
			playModeContainer.RemoveFromClassList(HiddenClassName);

			var noWorldInfo = playModeContainer.Q<Label>("NoWorldInfo");
			if (World == null)
			{
				noWorldInfo.RemoveFromClassList(HiddenClassName);
				return;
			}

			var world = World!;
			
			noWorldInfo.AddToClassList(HiddenClassName);

			playModeContainer.Q<WorldUIControl>().stringAttr = world.AllComponentsStorages.ToString();
		}
	}
}
