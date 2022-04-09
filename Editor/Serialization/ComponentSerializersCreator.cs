using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KVD.ECS.Core.Components;
using KVD.ECS.Core.Entities;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;
using Assembly = System.Reflection.Assembly;

namespace KVD.ECS.Editor.Serialization
{
	public static partial class ComponentSerializersCreator
	{
		private static readonly Type ComponentInterface = typeof(IComponent);
		private static readonly Type MonoComponentInterface = typeof(IMonoComponent);

		[MenuItem("KVD/ECS/Bake serializers")]
		private static void BakeSerializers()
		{
			var componentsTypes = CollectComponentsTypes();

			foreach (var o in Selection.objects)
			{
				Debug.Log(o.GetType());
			}

			// var playerSettings = new BakingSettings(
			// 	componentsTypes.PlayerComponents,
			// 	"KVD.DeusVult.ECS.Serializers",
			// 	Path.Combine(Application.dataPath, "Scripts/DeusVult/ECS/Serializers")
			// 	);
			//
			// BakeSerializers(playerSettings);
			//
			// var editorSettings = new BakingSettings(
			// 	componentsTypes.EditorComponents,
			// 	"KVD.Core.ECS.Tests.Serializers",
			// 	Path.Combine(Application.dataPath, "Scripts/Core/ECS/Tests/Serializers")
			// 	);
			//
			// BakeSerializers(editorSettings);
		}
		
		private static void BakeSerializers(BakingSettings bakingSettings)
		{

			if (!Directory.Exists(bakingSettings.SerializersPath))
			{
				Directory.CreateDirectory(bakingSettings.SerializersPath);
			}

			foreach (var structType in bakingSettings.ComponentTypes)
			{
				BakeSerializer(structType, bakingSettings);
			}
			// TODO: Generic way to create file from template
			var usings = new HashSet<string>
			{
				"System", "System.Collections.Generic", "KVD.Core.Serializers", "UnityEngine.Scripting",
			};
			StringBuilder stringBuilder = new();
			foreach (var structType in bakingSettings.ComponentTypes)
			{
				stringBuilder.AppendLine(RegisterSerializerTemplate.Replace(TypeID, structType.Name));
				if (bakingSettings.SerializersNamespace != structType.Namespace)
				{
					usings.Add(structType.Namespace);
				}
			}
			
			var serializersDeclaration = stringBuilder.ToString();
			stringBuilder.Clear();
			
			foreach (var usingName in usings)
			{
				var usingDefinition = UsingTemplate.Replace(UsingID, usingName);
				stringBuilder.AppendLine(usingDefinition);
			}
			
			var provider = SerializersProviderTemplate
				.Replace(NamespaceID, bakingSettings.SerializersNamespace)
				.Replace(RegisterSerializersID, serializersDeclaration)
				.Replace(UsingsID, stringBuilder.ToString());
			
			var path = Path.Combine(bakingSettings.SerializersPath, "SerializersProvider.cs");
			File.WriteAllText(path, provider);
			var relativePath = $"Assets{path.Substring(Application.dataPath.Length)}";
			AssetDatabase.ImportAsset(relativePath);
		}

		private static void BakeSerializer(Type structType, BakingSettings bakingSettings)
		{
			// Order of fields is not guaranteed but let's ignore it now and resolve later (when starts to be problem)
			var fields = structType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);

			var normalFields   = new List<FieldInfo>(fields.Length);
			var readonlyFields = new List<FieldInfo>(fields.Length);
			
			var usings             = new HashSet<string>
			{
				"System.IO", "UnityEngine", "KVD.Core.Serializers",
			};
			var serializers        = new HashSet<string>();
			var marshalSerializers = new HashSet<Type>();

			// TODO: StringBuilder 
			StringBuilder writeBuilder = new();
			StringBuilder readBuilder = new();
			
			usings.Add(structType.Namespace);

			foreach (var field in fields)
			{
				var fieldName = field.Name;
				usings.Add(field.FieldType.Namespace);
				
				if (field.IsInitOnly)
				{
					readonlyFields.Add(field);
				}
				else
				{
					normalFields.Add(field);
				}

				if (field.FieldType.IsPrimitive)
				{
					var fieldWrite = WriteWritePrimitiveTemplate.Replace(FieldNameID, fieldName);
					writeBuilder.AppendLine(fieldWrite);
					
					var fieldRead = ReadReadPrimitiveTemplate
						.Replace(FieldNameID, fieldName)
						.Replace(FieldTypeID, field.FieldType.Name);
					readBuilder.AppendLine(fieldRead);
				}
				else if (ComponentInterface.IsAssignableFrom(field.FieldType))
				{
					var serializer = $"{field.FieldType.Name}Serializer";
					serializers.Add(serializer);
					
					var fieldWrite = WriteWriteSerializerTemplate
						.Replace(FieldNameID, fieldName)
						.Replace(FieldSerializerID, serializer);
					writeBuilder.AppendLine(fieldWrite);
					
					var fieldRead = ReadReadSerializerTemplate
						.Replace(FieldNameID, fieldName)
						.Replace(FieldSerializerID, serializer);
					readBuilder.AppendLine(fieldRead);
				}
				else if (IsNativeArray(field.FieldType))
				{
					usings.Add(field.FieldType.GenericTypeArguments[0].Namespace);
					
					if (field.FieldType.GenericTypeArguments[0] == typeof(Entity))
					{
						var fieldWrite = WriteWriteNativeArrayEntityTemplate.Replace(FieldNameID, fieldName);
						writeBuilder.AppendLine(fieldWrite);
						
						var fieldRead = ReadReadNativeArrayEntityTemplate.Replace(FieldNameID, fieldName);
						readBuilder.AppendLine(fieldRead);
					}
					else if (ComponentInterface.IsAssignableFrom(field.FieldType.GenericTypeArguments[0]))
					{
						var fieldWrite = WriteWriteNativeArrayComponentTemplate.Replace(FieldNameID, fieldName);
						writeBuilder.AppendLine(fieldWrite);
						
						var fieldRead = ReadReadNativeArrayComponentTemplate
							.Replace(FieldNameID, fieldName)
							.Replace(FieldTypeID, field.FieldType.GenericTypeArguments[0].Name);
						readBuilder.AppendLine(fieldRead);
					}
					else
					{
						var fieldWrite = WriteWriteNativeArrayTemplate.Replace(FieldNameID, fieldName);
						writeBuilder.AppendLine(fieldWrite);
						
						var fieldRead = ReadReadNativeArrayTemplate
							.Replace(FieldNameID, fieldName)
							.Replace(FieldTypeID, field.FieldType.GenericTypeArguments[0].Name);
						readBuilder.AppendLine(fieldRead);
					}
				}
				else
				{
					marshalSerializers.Add(field.FieldType);
					
					var fieldWrite = WriteWriteMarshalTemplate.Replace(FieldNameID, fieldName);
					writeBuilder.AppendLine(fieldWrite);
					
					var fieldRead = ReadReadMarshalTemplate
						.Replace(FieldNameID, fieldName)
						.Replace(FieldTypeID, field.FieldType.Name);
					readBuilder.AppendLine(fieldRead);
				}
			}
			
			var typeName = structType.Name;

			var writeBytes = WriteBytesTemplate
				.Replace(TypeID, typeName)
				.Replace(WriteID, writeBuilder.ToString());
			
			writeBuilder.Clear();
			
			string fieldsInitializers = "";
			if (normalFields.Count > 0)
			{
				foreach (var fieldInfo in normalFields)
				{
					writeBuilder.AppendLine($"{fieldInfo.Name} = {fieldInfo.Name},");
				}
				fieldsInitializers = FieldsInitializerTemplate
					.Replace(FieldInitializerID, writeBuilder.ToString());
			}

			var constructor = ConstructorTemplate
				.Replace(TypeID, typeName)
				.Replace(ConstructorParametersID, string.Join(", ", readonlyFields.Select(fi => fi.Name)))
				.Replace(FieldsInitializerID, fieldsInitializers);

			var readBytes = ReadBytesTemplate
				.Replace(TypeID, typeName)
				.Replace(ReadID, readBuilder.ToString())
				.Replace(ConstructorID, constructor);

			writeBuilder.Clear();
			readBuilder.Clear();

			AssertConstructor(structType, readonlyFields);

			var serializersBuilder = writeBuilder;
			foreach (var serializer in serializers)
			{
				var serializerDefinition = SerializerTemplate.Replace(FieldSerializerID, serializer);
				serializersBuilder.AppendLine(serializerDefinition);
			}
			
			var usingsBuilder = readBuilder;
			foreach (var usingName in usings)
			{
				var usingDefinition = UsingTemplate.Replace(UsingID, usingName);
				usingsBuilder.AppendLine(usingDefinition);
			}

			var serializerFile = FileTemplate
				.Replace(NamespaceID, bakingSettings.SerializersNamespace)
				.Replace(TypeID, typeName)
				.Replace(AllFieldsSerializersDefinitionID, serializersBuilder.ToString())
				.Replace(UsingsID, usingsBuilder.ToString())
				.Replace(WriteBytesID, writeBytes)
				.Replace(ReadBytesID, readBytes);

			var path = Path.Combine(bakingSettings.SerializersPath, $"{typeName}Serializer.cs");
			File.WriteAllText(path, serializerFile);
			var relativePath = $"Assets{path.Substring(Application.dataPath.Length)}";
			AssetDatabase.ImportAsset(relativePath);
		}

		private static IEnumerable<Type> CollectComponentsTypes()
		{
			var allAssemblies   = AppDomain.CurrentDomain.GetAssemblies();

			return allAssemblies
				.SelectMany(a => a.GetTypes())
				.Where(t => IsComponentType(t) && t.IsPublic);

			static bool IsComponentType(Type type)
			{
				return ComponentInterface.IsAssignableFrom(type);
			}
		}

		private static void AssertConstructor(Type structType, IReadOnlyCollection<FieldInfo> readonlyFields)
		{
			if (readonlyFields.Count <= 0)
			{
				return;
			}
			
			var constructor = structType.GetConstructor(readonlyFields.Select(fi => fi.FieldType).ToArray());
			Assert.IsNotNull(constructor);
		}
		
		private static bool IsNativeArray(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				return type.GetGenericTypeDefinition() == typeof(NativeArray<>);
			}
			return false;
		}

		private class BakingSettings
		{
			public string SerializersPath{ get; set; }
			public string SerializersNamespace{ get; set; }
			public IEnumerable<Type> ComponentTypes{ get; set; }
			
			public BakingSettings(IEnumerable<Type> componentTypes, string serializersNamespace, string serializersPath)
			{
				SerializersPath      = serializersPath;
				SerializersNamespace = serializersNamespace;
				ComponentTypes       = componentTypes;
			}
		}
	}
}
