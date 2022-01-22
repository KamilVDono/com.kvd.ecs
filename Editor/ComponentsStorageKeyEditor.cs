using System;
using KVD.ECS.Core;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace KVD.ECS.Editor
{
	[CustomPropertyDrawer(typeof(ComponentsStorageKeyAuthoring), true)]
	public class ComponentsStorageKeyEditor : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var hashProp    = property.FindPropertyRelative("_hash");
			var valueIndex  = Array.IndexOf(ComponentsStorageKey.AllValues, new ComponentsStorageKey(hashProp.intValue));

			EditorGUI.BeginChangeCheck();
			var newSelection = EditorGUI.Popup(position, valueIndex, ComponentsStorageKey.AllNames);
			if (EditorGUI.EndChangeCheck())
			{
				hashProp.intValue = ComponentsStorageKey.AllValues[newSelection].GetHashCode();
			}
			
			EditorGUI.EndProperty();
		}
	}
}
