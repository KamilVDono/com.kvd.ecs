using System;

#nullable enable

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class TableColumn<T>
	{
		private readonly Func<T, string> _valueGetter;
		public string Name{ get; }
		public float WidthMultiplier{ get; }
		
		public TableColumn(string name, Func<T, string> valueGetter, float widthMultiplier)
		{
			Name            = name;
			WidthMultiplier = widthMultiplier;
			_valueGetter    = valueGetter;
		}

		public string Value(T target)
		{
			return _valueGetter(target);
		}
	}
}
