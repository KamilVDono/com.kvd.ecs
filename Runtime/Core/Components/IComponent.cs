using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using KVD.ECS.Core.Helpers;
using KVD.Utils.DataStructures;
using UnityEngine;

namespace KVD.ECS.Core.Components
{
	public interface IComponent : IDisposable
	{
		private static readonly MethodInfo GetMethodInfo = typeof(RentedArray<>).GetMethod("Get",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
		private static readonly MethodInfo SetMethodInfo = typeof(RentedArray<>).GetMethod("Set",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
		private static readonly PropertyInfo LengthPropertyInfo = typeof(RentedArray<>).GetProperty("Length",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
		private static readonly SimplePool<StringBuilder> StringBuilders = new(4, () => new());
		
		void IDisposable.Dispose() {}

		// TODO: This should be done by component serialization sources generators
		// TODO: BUT Unity implementation has bugs and not all projects are affected by sources generators
		// So I could write my own, fast and frugal serialization baker
		// but I'll postpone this and maybe source generators will be working one more time 
		void Serialize(BinaryWriter writer)
		{
			var stringSerialized = StringSerialization();
			writer.Write(stringSerialized);
		}

		string StringSerialization()
		{
			using var serializationHolderBorrow = LikeDictionarySerialization.Pool.Borrow();

			var serializationHolder = serializationHolderBorrow.Element;
			serializationHolder.Clear();
			var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				var fieldValue = field.GetValue(this);
				if (field.FieldType.IsPrimitive)
				{
					serializationHolder.Add(field.Name, fieldValue);
				}
				else if (typeof(IComponent).IsAssignableFrom(field.FieldType))
				{
					serializationHolder.Add(field.Name, ((IComponent)fieldValue).StringSerialization());
				}
				else if (field.FieldType.IsValueType)
				{
					serializationHolder.Add(field.Name, JsonUtility.ToJson(fieldValue));
				}
				else if (field.FieldType == typeof(string))
				{
					serializationHolder.Add(field.Name, fieldValue);
				}
				else if (field.FieldType.IsArray)
				{
					var array = (Array)fieldValue;
					if (field.FieldType.GetElementType().IsPrimitive)
					{
						serializationHolder.Add(field.Name, string.Join("|", array));
					}
					else if (typeof(IComponent).IsAssignableFrom(field.FieldType.GetElementType()))
					{
						using var stringBuilderBorrow = StringBuilders.Borrow();
						var       stringBuilder       = stringBuilderBorrow.Element;
						stringBuilder.Clear();
						foreach (var elem in array)
						{
							stringBuilder.Append(((IComponent)elem).StringSerialization());
							stringBuilder.Append('|');
						}
						if (stringBuilder.Length > 0) stringBuilder.Length -= 1;
						serializationHolder.Add(field.Name, stringBuilder.ToString());
					}
					else if (field.FieldType.GetElementType().IsValueType)
					{
						using var stringBuilderBorrow = StringBuilders.Borrow();
						var       stringBuilder       = stringBuilderBorrow.Element;
						stringBuilder.Clear();
						foreach (var elem in array)
						{
							stringBuilder.Append(JsonUtility.ToJson(elem));
							stringBuilder.Append('|');
						}
						if (stringBuilder.Length > 0) stringBuilder.Length -= 1;
						serializationHolder.Add(field.Name, stringBuilder.ToString());
					}
				}
				else if (typeof(RentedArray<>).IsAssignableFrom(field.FieldType))
				{
					var length     = (int)LengthPropertyInfo.GetValue(fieldValue);
					var serialized = new RentedArraySerialization
					{
						length = length,
					};
					var parameters = new object[1];
					if (field.FieldType.GetGenericArguments()[0].IsPrimitive)
					{
						using var stringBuilderBorrow = StringBuilders.Borrow();
						var       stringBuilder       = stringBuilderBorrow.Element;
						stringBuilder.Clear();
						for (var i = 0; i < length; i++)
						{
							parameters[0] = i;
							stringBuilder.Append(GetMethodInfo.Invoke(fieldValue, parameters));
							stringBuilder.Append('|');
						}
						if (stringBuilder.Length > 0) stringBuilder.Length -= 1;
						serialized.values    =  stringBuilder.ToString();
					}
					else if (typeof(IComponent).IsAssignableFrom(field.FieldType.GetElementType()))
					{
						using var stringBuilderBorrow = StringBuilders.Borrow();
						var       stringBuilder       = stringBuilderBorrow.Element;
						stringBuilder.Clear();
						for (var i = 0; i < length; i++)
						{
							parameters[0] = i;
							stringBuilder.Append(((IComponent)GetMethodInfo.Invoke(fieldValue, parameters))
								.StringSerialization());
							stringBuilder.Append('|');
						}
						if (stringBuilder.Length > 0) stringBuilder.Length -= 1;
						serialized.values = stringBuilder.ToString();
					}
					else if (field.FieldType.GetElementType().IsValueType)
					{
						using var stringBuilderBorrow = StringBuilders.Borrow();
						var       stringBuilder       = stringBuilderBorrow.Element;
						stringBuilder.Clear();
						for (var i = 0; i < length; i++)
						{
							parameters[0] = i;
							stringBuilder.Append(JsonUtility.ToJson(GetMethodInfo.Invoke(fieldValue, parameters)));
							stringBuilder.Append('|');
						}
						if (stringBuilder.Length > 0) stringBuilder.Length -= 1;
						serialized.values = stringBuilder.ToString();
					}
					serializationHolder.Add(field.Name, JsonUtility.ToJson(serialized));
				}
			}
			return JsonUtility.ToJson(serializationHolder);
		}
		
		IComponent Deserialize(BinaryReader reader)
		{
			var json = reader.ReadString();
			Deserialize(json);
			return this;
		}
		
		void Deserialize(string stringValue)
		{
			var myValues = JsonUtility.FromJson<LikeDictionarySerialization>(stringValue);
			var type     = GetType();
			for (var i = 0; i < myValues.fields.Count; i++)
			{
				var fieldName  = myValues.fields[i];
				var fieldValue = myValues.fieldValues[i];
				var field = type.GetField(fieldName,
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField);
				if (field == null)
				{
					Debug.LogWarning($"Can not find field [{fieldName}] in type [{type.Name}]");
					continue;
				}
				
				if (field.FieldType.IsPrimitive)
				{
					var m = field.FieldType.GetMethod("Parse", new[] { typeof(string), });
					field.SetValue(this, m.Invoke(null, new object[] { fieldValue, }));
				}
				else if (typeof(IComponent).IsAssignableFrom(field.FieldType))
				{
					var component = (IComponent)Activator.CreateInstance(field.FieldType);
					component.Deserialize(fieldValue);
					field.SetValue(this, component);
				}
				else if (field.FieldType.IsValueType)
				{
					var structValue = JsonUtility.FromJson(fieldValue, field.FieldType);
					field.SetValue(this, structValue);
				}
				else if (field.FieldType == typeof(string))
				{
					field.SetValue(this, fieldValue);
				}
				else if (field.FieldType.IsArray)
				{
					var values = fieldValue.Split('|');
					var array  = Array.CreateInstance(field.FieldType.GetElementType(), values.Length);
					if (field.FieldType.GetElementType().IsPrimitive)
					{
						var m = field.FieldType.GetMethod("Parse", new[] { typeof(string), });
						for (var j = 0; j < values.Length; j++)
						{
							array.SetValue(m.Invoke(null, new object[] { values[j], }), j);
						}
					}
					else if (typeof(IComponent).IsAssignableFrom(field.FieldType.GetElementType()))
					{
						var componentType = field.FieldType.GetElementType();
						for (var j = 0; j < values.Length; j++)
						{
							var component = (IComponent)Activator.CreateInstance(componentType);
							component.Deserialize(values[j]);
							array.SetValue(component, j);
						}
					}
					else if (field.FieldType.GetElementType().IsValueType)
					{
						var structType = field.FieldType.GetElementType();
						for (var j = 0; j < values.Length; j++)
						{
							array.SetValue(JsonUtility.FromJson(values[j], structType), j);
						}
					}
					field.SetValue(this, array);
				}
				else if (typeof(RentedArray<>).IsAssignableFrom(field.FieldType))
				{
					var elementType = field.FieldType.GetGenericArguments()[0];
					var arrayType   = typeof(RentedArray<>).MakeGenericType(elementType);

					var serialized = JsonUtility.FromJson<RentedArraySerialization>(fieldValue);
					
					var length      = serialized.length;
					var values      = serialized.values.Split('|');
					var rentedArray = Activator.CreateInstance(arrayType, length);

					if (field.FieldType.GetElementType().IsPrimitive)
					{
						var m = field.FieldType.GetMethod("Parse", new[] { typeof(string), });
						for (var j = 0; j < values.Length; j++)
						{
							SetMethodInfo.Invoke(rentedArray,
								new[] { m.Invoke(null, new object[] { values[j], }), j });
						}
					}
					else if (typeof(IComponent).IsAssignableFrom(field.FieldType.GetElementType()))
					{
						var componentType = field.FieldType.GetElementType();
						for (var j = 0; j < values.Length; j++)
						{
							var component = (IComponent)Activator.CreateInstance(componentType);
							component.Deserialize(values[j]);
							SetMethodInfo.Invoke(rentedArray, new object[] { component, j });
						}
					}
					else if (field.FieldType.GetElementType().IsValueType)
					{
						var structType = field.FieldType.GetElementType();
						for (var j = 0; j < values.Length; j++)
						{
							SetMethodInfo.Invoke(rentedArray,
								new[] { JsonUtility.FromJson(values[j], structType), j });
						}
					}
					field.SetValue(this, rentedArray);
				}
			}
		}

		[Serializable]
		internal class LikeDictionarySerialization
		{
			public static readonly SimplePool<LikeDictionarySerialization> Pool = new(4, () => new());
			
			public List<string> fields = new();
			public List<string> fieldValues = new();

			public void Add(string field, string value)
			{
				fields.Add(field);
				fieldValues.Add(value);
			}
			
			public void Add(string field, object value)
			{
				fields.Add(field);
				fieldValues.Add(value.ToString());
			}

			public void Clear()
			{
				fields.Clear();
				fieldValues.Clear();
			}
		}
		
		[Serializable]
		internal class RentedArraySerialization
		{
			public int length;
			public string values;
		}
	}
}
