﻿// <auto-generated />

#pragma warning disable 618, 612, 414, 168, CS1591, SA1129, SA1309, SA1312, SA1403, SA1649

namespace MessagePack;

using MsgPack = global::MessagePack;

partial class GeneratedMessagePackResolver
{
	private sealed class MyEnumFormatter : MsgPack::Formatters.IMessagePackFormatter<global::MyTestNamespace.MyEnum>
	{
		public void Serialize(ref MsgPack::MessagePackWriter writer, global::MyTestNamespace.MyEnum value, MsgPack::MessagePackSerializerOptions options)
		{
			writer.Write((int)value);
		}

		public global::MyTestNamespace.MyEnum Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
		{
			return (global::MyTestNamespace.MyEnum)reader.ReadInt32();
		}
	}
}
