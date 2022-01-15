using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class TableView<T>
	{
		private float _width;
		private readonly TableColumn<T>[] _columns;

		public TableView(TableColumn<T>[] columns)
		{
			_columns = columns;
		}
		
		public void Begin(float width)
		{
			_width = width * 0.95f;
			GUILayout.BeginVertical();
		}
		
		public void End()
		{
			GUILayout.EndVertical();
		}

		public void DrawHeader()
		{
			GUILayout.BeginHorizontal();
			foreach (var column in _columns)
			{
				GUILayout.Box(column.Name, GUILayout.Width(_width*column.WidthMultiplier));
			}
			GUILayout.EndHorizontal();
		}
		
		public void DrawRow(T rowObject)
		{
			GUILayout.BeginHorizontal();
			foreach (var column in _columns)
			{
				GUILayout.Box(column.Value(rowObject), GUILayout.Width(_width*column.WidthMultiplier));
			}
			GUILayout.EndHorizontal();
		}
		
		public void DrawFull(IEnumerable<T?> values, float width, bool skipNulls = true)
		{
			Begin(width);
			DrawHeader();
			foreach (var rowValue in values)
			{
				if (skipNulls && rowValue == null)
				{
					continue;
				}
#pragma warning disable 8604
				DrawRow(rowValue);
#pragma warning restore 8604
			}
			End();
		}
	}
}
