using System;
using System.Collections.Generic;
using System.Reflection;
using KVD.ECS.Systems;
using Unity.Profiling;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class SystemWrapper : IDisposable
	{
		private static readonly FieldInfo ComponentsViewsField = typeof(SystemBase).GetField("_componentsViews", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
		private static readonly FieldInfo UpdateMarkerField = typeof(SystemBase).GetField("_updateMarker", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
			
		public SystemBase BackingSystem{ get; }
		
		public string DisplayName{ get; }
		public IReadOnlyCollection<IComponentsView> ComponentsViews{ get; }
		public ProfilerMarker UpdateMarker{ get; }
		public ProfilerRecorder Recorder{ get; }

		public SystemWrapper(SystemBase backingSystem)
		{
			BackingSystem   = backingSystem;
			DisplayName     = BackingSystem.GetType().Name;
			ComponentsViews = (IReadOnlyCollection<IComponentsView>)ComponentsViewsField.GetValue(BackingSystem);
			UpdateMarker    = (ProfilerMarker)UpdateMarkerField.GetValue(BackingSystem);
			Recorder        = ProfilerRecorder.StartNew(UpdateMarker, 5);
		}
		
		public void Dispose()
		{
			Recorder.Dispose();
		}
	}
}
