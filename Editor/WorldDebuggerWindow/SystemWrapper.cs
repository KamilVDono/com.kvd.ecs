using System;
using System.Collections.Generic;
using System.Reflection;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using Unity.Profiling;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class SystemWrapper : IDisposable
	{
		private static readonly FieldInfo ComponentsViewsField = typeof(SystemBase).GetField("_componentsViews", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
		private static readonly FieldInfo UpdateMarkerFieldSystemBase = typeof(SystemBase).GetField("_updateMarker", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
		private static readonly FieldInfo UpdateMarkerFieldSystemGroup = typeof(SystemsGroup).GetField("_updateMarker", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;

		private ISystem BackingSystem{ get; }
		
		public string DisplayName{ get; }
		public IReadOnlyCollection<IComponentsView>? ComponentsViews{ get; }
		public ProfilerMarker UpdateMarker{ get; }
		public ProfilerRecorder Recorder{ get; }

		public SystemWrapper(ISystem backingSystem)
		{
			BackingSystem   = backingSystem;
			if (backingSystem is SystemBase)
			{
				DisplayName     = BackingSystem.GetType().Name;
				ComponentsViews = (IReadOnlyCollection<IComponentsView>)ComponentsViewsField.GetValue(BackingSystem);
				UpdateMarker    = (ProfilerMarker)UpdateMarkerFieldSystemBase.GetValue(BackingSystem);
				Recorder        = ProfilerRecorder.StartNew(UpdateMarker, 5);
			}
			else if (backingSystem is SystemsGroup systemsGroup)
			{
				DisplayName  = systemsGroup.Name;
				UpdateMarker = (ProfilerMarker)UpdateMarkerFieldSystemGroup.GetValue(BackingSystem);
				Recorder     = ProfilerRecorder.StartNew(UpdateMarker, 5);
			}
			else
			{
				DisplayName = "Unknown";
			}
		}
		
		public void Dispose()
		{
			Recorder.Dispose();
		}
	}
}
