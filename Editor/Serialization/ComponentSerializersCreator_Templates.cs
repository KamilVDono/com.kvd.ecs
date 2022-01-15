namespace KVD.ECS.Editor.Serialization
{
	public static partial class ComponentSerializersCreator
	{
		private const string TypeID = "{TYPE}";
		private const string UsingsID = "{USINGS}";
		private const string NamespaceID = "{NAMESPACE}";
		private const string FieldNameID = "{FIELD_NAME}";
		private const string FieldSerializerID = "{FIELD_SERIALIZER}";
		private const string FieldTypeID = "{FIELD_TYPE}";
		private const string ConstructorParametersID = "{CONSTRUCTOR_PARAMETERS}";
		private const string FieldsInitializerID = "{FIELDS_INITIALIZER}";
		private const string FieldInitializerID = "{FIELD_INITIALIZER}";
		private const string WriteID = "{WRITE}";
		private const string ReadID = "{READ}";
		private const string ConstructorID = "{CONSTRUCTOR}";
		private const string AllFieldsSerializersDefinitionID = "{ALL_FIELDS_SERIALIZERS_DEFINITION}";
		private const string WriteBytesID = "{WRITE_BYTES_FUNCTION}";
		private const string ReadBytesID = "{READ_BYTES_FUNCTION}";
		private const string UsingID = "{USING_NAMESPACE}";
		private const string RegisterSerializersID = "{REGISTER_SERIALIZERS}";

		private static readonly string SerializerTemplate = @$"private static readonly {FieldSerializerID} {FieldSerializerID} = new();";
		private static readonly string UsingTemplate = @$"using {UsingID};";
		private static readonly string WriteBytesTemplate = @$"
		public void WriteBytes({TypeID} target, BinaryWriter writer)
		{{
			{WriteID}
		}}
";
		private static readonly string WriteWritePrimitiveTemplate = @$"writer.Write(target.{FieldNameID});";
		private static readonly string WriteWriteMarshalTemplate = @$"SerializersHelper.ToMarshalBytes(target.{FieldNameID}, writer);";
		private static readonly string WriteWriteSerializerTemplate = @$"{FieldSerializerID}.WriteBytes(target.{FieldNameID}, writer);";
		private static readonly string WriteWriteNativeArrayTemplate = @$"SerializersHelper.ToBytesNativeArray(target.{FieldNameID}, writer);";
		private static readonly string WriteWriteNativeArrayComponentTemplate = @$"SerializersHelper.ToBytesComponentNativeArrayComponent(target.{FieldNameID}, writer);";
		private static readonly string WriteWriteNativeArrayEntityTemplate = @$"SerializersHelper.ToBytesNativeArrayEntity(target.{FieldNameID}, writer);";
		private static readonly string ReadBytesTemplate = @$"
		public {TypeID} ReadBytes(BinaryReader reader)
		{{
			{ReadID}

			{ConstructorID}

			return deserializedStruct;
		}}
";
		private static readonly string ReadReadPrimitiveTemplate = @$"var {FieldNameID} = reader.Read{FieldTypeID}();";
		private static readonly string ReadReadMarshalTemplate = @$"var {FieldNameID} = SerializersHelper.FromMarshalBytes<{FieldTypeID}>(reader);";
		private static readonly string ReadReadSerializerTemplate = @$"var {FieldNameID} = {FieldSerializerID}.ReadBytes(reader);";
		private static readonly string ReadReadNativeArrayTemplate = @$"var {FieldNameID} = SerializersHelper.FromBytesNativeArray<{FieldTypeID}>(reader);";
		private static readonly string ReadReadNativeArrayComponentTemplate = @$"var {FieldNameID} = SerializersHelper.FromBytesNativeArrayComponent<{FieldTypeID}>(reader);";
		private static readonly string ReadReadNativeArrayEntityTemplate = @$"var {FieldNameID} = SerializersHelper.FromBytesNativeArrayEntity(reader);";
		private static readonly string ConstructorTemplate = @$"var deserializedStruct = new {TypeID}({ConstructorParametersID}){FieldsInitializerID};";
		private static readonly string FieldsInitializerTemplate = @$"
{{
				{FieldInitializerID}
}}";
		private static readonly string FileTemplate = @$"{UsingsID}

namespace {NamespaceID}
{{
	public class {TypeID}Serializer : IComponentSerializer<{TypeID}>
	{{
		{AllFieldsSerializersDefinitionID}
		{WriteBytesID}
		{ReadBytesID}
	}}
}}";

		private static readonly string RegisterSerializerTemplate = $@"serializers[typeof({TypeID})] = new {TypeID}Serializer();";
		private static readonly string SerializersProviderTemplate = $@"{UsingsID}

namespace {NamespaceID}
{{
	[Preserve]
	public class SerializersProvider : ISerializersProvider
	{{
		public void FillSerializers(Dictionary<Type, IComponentSerializer> serializers)
		{{
			{RegisterSerializersID}
		}}
	}}
}}";
	}
}
