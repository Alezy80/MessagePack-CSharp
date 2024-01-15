﻿// <auto-generated />

#pragma warning disable 618, 612, 414, 168, CS1591, SA1129, SA1309, SA1312, SA1403, SA1649

#pragma warning disable CS8669 // We may leak nullable annotations into generated code.

namespace MessagePack;

using MsgPack = global::MessagePack;

partial class GeneratedMessagePackResolver
{
	private sealed class MyGenericTypeFormatter<T> : MsgPack::Formatters.IMessagePackFormatter<global::MyGenericType<T>>
	{

		public void Serialize(ref MsgPack::MessagePackWriter writer, global::MyGenericType<T> value, MsgPack::MessagePackSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNil();
				return;
			}

			MsgPack::IFormatterResolver formatterResolver = options.Resolver;
			writer.WriteArrayHeader(1);
			MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<T>(formatterResolver).Serialize(ref writer, value.Value, options);
		}

		public global::MyGenericType<T> Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
		{
			if (reader.TryReadNil())
			{
				return null;
			}

			options.Security.DepthStep(ref reader);
			MsgPack::IFormatterResolver formatterResolver = options.Resolver;
			var length = reader.ReadArrayHeader();
			var ____result = new global::MyGenericType<T>();

			for (int i = 0; i < length; i++)
			{
				switch (i)
				{
					case 0:
						____result.Value = MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<T>(formatterResolver).Deserialize(ref reader, options);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			reader.Depth--;
			return ____result;
		}
	}
}
