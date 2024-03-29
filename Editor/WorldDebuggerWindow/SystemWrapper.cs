using System;
using System.Collections.Generic;
using System.Reflection;
using KVD.ECS.Core;
using KVD.ECS.Core.Systems;
using KVD.ECS.SystemHelpers;
#if SYSTEM_PROFILER_MARKERS
using Unity.Profiling;
#endif

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class SystemWrapper : IDisposable
	{
#if SYSTEM_PROFILER_MARKERS
		static readonly FieldInfo UpdateMarkerFieldSystemBase = typeof(SystemBase).GetField("_updateMarker", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
		static readonly FieldInfo UpdateMarkerFieldSystemGroup = typeof(SystemsGroup).GetField("_updateMarker", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)!;
#endif
		public bool expanded = true;

		public string DisplayName{ get; }
#if SYSTEM_PROFILER_MARKERS
		public ProfilerMarker UpdateMarker{ get; }
		public ProfilerRecorder Recorder{ get; }
#endif
		public int Depth{ get; }
		
		ISystem BackingSystem{ get; }

		public SystemWrapper(ISystem backingSystem, int depth)
		{
			Depth         = depth;
			BackingSystem = backingSystem;
			
			if (BackingSystem is SystemBase)
			{
				DisplayName     = BackingSystem.GetType().Name;
#if SYSTEM_PROFILER_MARKERS
				UpdateMarker    = (ProfilerMarker)UpdateMarkerFieldSystemBase.GetValue(BackingSystem);
				Recorder        = ProfilerRecorder.StartNew(UpdateMarker, 50);
#endif
			}
			else if (BackingSystem is SystemsGroup systemsGroup)
			{
				DisplayName = systemsGroup.Name;
#if SYSTEM_PROFILER_MARKERS
				UpdateMarker = (ProfilerMarker)UpdateMarkerFieldSystemGroup.GetValue(BackingSystem);
				Recorder     = ProfilerRecorder.StartNew(UpdateMarker, 50);
#endif
			}
			else
			{
				DisplayName = "Unknown";
			}
		}
		
		public void Dispose()
		{
#if SYSTEM_PROFILER_MARKERS
			Recorder.Dispose();
#endif
		}
	}
}
