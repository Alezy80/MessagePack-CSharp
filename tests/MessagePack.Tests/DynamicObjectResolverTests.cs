﻿// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !(MESSAGEPACK_FORCE_AOT || ENABLE_IL2CPP)
#define DYNAMIC_GENERATION
#endif

#if DYNAMIC_GENERATION

using System;
using System.Runtime.Serialization;
using MessagePack.Resolvers;
using Nerdbank.Streams;
using Xunit;
using Xunit.Abstractions;

namespace MessagePack.Tests
{
    public class DynamicObjectResolverTests
    {
        private readonly ITestOutputHelper logger;

        public DynamicObjectResolverTests(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        [Fact]
        public void SerializeTypeWithReadOnlyField()
        {
            Assert3MemberClassSerializedContent(MessagePackSerializer.Serialize(new TestMessageWithReadOnlyField()));
        }

        [Fact]
        public void SerializeTypeWithReadOnlyProperty()
        {
            Assert3MemberClassSerializedContent(MessagePackSerializer.Serialize(new TestMessageWithReadOnlyProperty()));
        }

        [Fact]
        public void SerializeTypeWithReadWriteFields()
        {
            Assert3MemberClassSerializedContent(MessagePackSerializer.Serialize(new TestMessageWithReadWriteFields()));
        }

        [Fact]
        public void PrivateMembersInBaseClass_StandardResolverAllowPrivate()
        {
            var options = StandardResolverAllowPrivate.Options;
            PrivateMembersInBaseClass_Helper(options);
        }

        [Fact]
        public void DeserializerSetsMissingPropertiesToDefaultValue()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(1);
            writer.Write(nameof(TwoProperties.Prop1));
            writer.Write("Set");
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<TwoProperties>(seq);
            Assert.Equal("Set", instance.Prop1);
            Assert.Equal("Uninitialized", instance.Prop2);
        }

        [Fact]
        public void DeserializerSetsMissingPropertiesToDefaultValue_OrdinalKey()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(1);
            writer.Write("Set");
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<TwoPropertiesOrdinalKey>(seq);
            Assert.Equal("Set", instance.Prop1);
            Assert.Equal("Uninitialized", instance.Prop2);
        }

        [Fact]
        public void CtorParameterAndPropertySetterExists()
        {
            var m1 = new ClassWithPropertySetterAndDummyCtor(999) { MyProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(m1);
            var m2 = MessagePackSerializer.Deserialize<ClassWithPropertySetterAndDummyCtor>(bin);

            // We do NOT use the property setter since the constructor is expected to set the property.
            Assert.Equal(0, m2.MyProperty);
        }

        /// <summary>
        /// Verifies that virtual and overridden properties do not cause the dynamic resolver to malfunction.
        /// </summary>
        [Fact]
        public void VirtualOverriddenProperties()
        {
            var obj = new DerivedClassThatOverridesProperty { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesProperty>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        [Fact]
        public void VirtualOverriddenProperties_DataMemberOnBase()
        {
            var obj = new DerivedClassThatOverridesPropertyDataMemberOnVirtualOnly { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesPropertyDataMemberOnVirtualOnly>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        [Fact]
        public void VirtualOverriddenProperties_DataMemberOnOverride()
        {
            var obj = new DerivedClassThatOverridesPropertyDataMemberOnOverrideOnly { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesPropertyDataMemberOnOverrideOnly>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        /// <summary>
        /// Verifies that virtual and overridden properties do not cause the dynamic resolver to malfunction.
        /// </summary>
        [Fact]
        public void VirtualOverriddenProperties_OrdinalKey()
        {
            var obj = new DerivedClassThatOverridesPropertyOrdinalKey { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesPropertyOrdinalKey>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        [Fact]
        public void VirtualOverriddenProperties_DataMemberOnBase_OrdinalKey()
        {
            var obj = new DerivedClassThatOverridesPropertyDataMemberOnVirtualOnlyOrdinalKey { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesPropertyDataMemberOnVirtualOnlyOrdinalKey>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        [Fact]
        public void VirtualOverriddenProperties_DataMemberOnOverride_OrdinalKey()
        {
            var obj = new DerivedClassThatOverridesPropertyDataMemberOnOverrideOnlyOrdinalKey { VirtualProperty = 100 };
            byte[] bin = MessagePackSerializer.Serialize(obj);
            var obj2 = MessagePackSerializer.Deserialize<DerivedClassThatOverridesPropertyDataMemberOnOverrideOnlyOrdinalKey>(bin);
            Assert.Equal(obj.VirtualProperty, obj2.VirtualProperty);
        }

        private static void Assert3MemberClassSerializedContent(ReadOnlyMemory<byte> msgpack)
        {
            var reader = new MessagePackReader(msgpack);
            Assert.Equal(3, reader.ReadArrayHeader());
            Assert.Equal(1, reader.ReadInt32());
            Assert.Equal(2, reader.ReadInt32());
            Assert.Equal(3, reader.ReadInt32());
        }

        private void PrivateMembersInBaseClass_Helper(MessagePackSerializerOptions options)
        {
            var obj = new DerivedClass { Name = "name", BaseClassFieldAccessor = 15, BaseClassProperty = 5 };
            var bin = MessagePackSerializer.Serialize(obj, options);
            this.logger.WriteLine(MessagePackSerializer.ConvertToJson(bin));
            var obj2 = MessagePackSerializer.Deserialize<DerivedClass>(bin, options);
            Assert.Equal(obj.BaseClassFieldAccessor, obj2.BaseClassFieldAccessor);
            Assert.Equal(obj.BaseClassProperty, obj2.BaseClassProperty);
            Assert.Equal(obj.Name, obj2.Name);
        }

        [Fact]
        public void DefaultValueStringKeyClassWithoutExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(0);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueStringKeyClassWithoutExplicitConstructor>(seq);
            Assert.Equal(DefaultValueStringKeyClassWithoutExplicitConstructor.Prop1Constant, instance.Prop1);
            Assert.Equal(DefaultValueStringKeyClassWithoutExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueStringKeyClassWithExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(1);
            writer.Write(nameof(DefaultValueStringKeyClassWithExplicitConstructor.Prop1));
            writer.Write(-1);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueStringKeyClassWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(DefaultValueStringKeyClassWithExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueStringKeyClassWithExplicitConstructorSetPropertyTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(2);
            writer.Write(nameof(DefaultValueStringKeyClassWithExplicitConstructor.Prop1));
            writer.Write(-1);
            writer.Write(nameof(DefaultValueStringKeyClassWithExplicitConstructor.Prop2));
            writer.Write(int.MaxValue);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueStringKeyClassWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(int.MaxValue, instance.Prop2);
        }

        [Fact]
        public void DefaultValueStringKeyStructWithExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(1);
            writer.Write(nameof(DefaultValueStringKeyStructWithExplicitConstructor.Prop1));
            writer.Write(-1);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueStringKeyStructWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(DefaultValueStringKeyStructWithExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueStringKeyStructWithExplicitConstructorSetPropertyTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteMapHeader(2);
            writer.Write(nameof(DefaultValueStringKeyStructWithExplicitConstructor.Prop1));
            writer.Write(-1);
            writer.Write(nameof(DefaultValueStringKeyStructWithExplicitConstructor.Prop2));
            writer.Write(int.MinValue);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueStringKeyStructWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(int.MinValue, instance.Prop2);
        }

        [Fact]
        public void DefaultValueIntKeyClassWithoutExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(0);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueIntKeyClassWithoutExplicitConstructor>(seq);
            Assert.Equal(DefaultValueIntKeyClassWithoutExplicitConstructor.Prop1Constant, instance.Prop1);
            Assert.Equal(DefaultValueIntKeyClassWithoutExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueIntKeyClassWithExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(1);
            writer.Write(-1);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueIntKeyClassWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(DefaultValueIntKeyClassWithExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueIntKeyClassWithExplicitConstructorSetPropertyTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(2);
            writer.Write(-1);
            writer.Write(42);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueIntKeyClassWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(42, instance.Prop2);
        }

        [Fact]
        public void DefaultValueIntKeyStructWithExplicitConstructorTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(1);
            writer.Write(-1);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueIntKeyStructWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(DefaultValueIntKeyStructWithExplicitConstructor.Prop2Constant, instance.Prop2);
        }

        [Fact]
        public void DefaultValueIntKeyStructWithExplicitConstructorSetPropertyTest()
        {
            var seq = new Sequence<byte>();
            var writer = new MessagePackWriter(seq);
            writer.WriteArrayHeader(2);
            writer.Write(-1);
            writer.Write(-98);
            writer.Flush();

            var instance = MessagePackSerializer.Deserialize<DefaultValueIntKeyStructWithExplicitConstructor>(seq);
            Assert.Equal(-1, instance.Prop1);
            Assert.Equal(-98, instance.Prop2);
        }

        [Fact]
        public void TestMapModeAgainstVariousVisibility()
        {
            ClassWithUseMapAndMembersOfVariousVisibility original = new()
            {
                PublicProperty = 1,
                InternalProperty = 2,
                PublicField = 4,
                InternalField = 5,
            };
            original.PrivateFieldAccessor() = 3;
            original.SetPrivateProperty(6);

            ClassWithUseMapAndMembersOfVariousVisibility deserialized = Roundtrip(original, MessagePackSerializerOptions.Standard);
            Assert.Equal(original.PublicProperty, deserialized.PublicProperty);
            Assert.Equal(original.PublicField, deserialized.PublicField);
            Assert.Equal(0, deserialized.InternalProperty);
            Assert.Equal(0, deserialized.InternalField);
            Assert.Equal(0, deserialized.GetPrivateProperty());
            Assert.Equal(0, deserialized.PrivateFieldAccessor());
        }

        [Fact]
        public void TestMapModeAgainstVariousVisibility_AllowPrivateAttribute()
        {
            ClassWithUseMapAndMembersOfVariousVisibilityAllowPrivate original = new()
            {
                PublicProperty = 1,
                InternalProperty = 2,
                PublicField = 4,
                InternalField = 5,
            };
            original.PrivateFieldAccessor() = 3;
            original.SetPrivateProperty(6);

            ClassWithUseMapAndMembersOfVariousVisibilityAllowPrivate deserialized = Roundtrip(original, MessagePackSerializerOptions.Standard);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void TestMapModeAgainstVariousVisibility_AllowPrivateResolver()
        {
            ClassWithUseMapAndMembersOfVariousVisibility original = new()
            {
                PublicProperty = 1,
                InternalProperty = 2,
                PublicField = 4,
                InternalField = 5,
            };
            original.PrivateFieldAccessor() = 3;
            original.SetPrivateProperty(6);

            ClassWithUseMapAndMembersOfVariousVisibility deserialized = Roundtrip(original, StandardResolverAllowPrivate.Options);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void InternalTypeWithPublicMembers()
        {
            var original = new InternalClassWithPublicMembers { PublicProperty = 3 };

            // We expect serialization to fail because we're not using the AllowPrivate resolver,
            // nor is the type attributed with the AllowPrivate setting.
            Assert.Throws<MessagePackSerializationException>(() => MessagePackSerializer.Serialize(original, MessagePackSerializerOptions.Standard));
        }

        [MessagePackObject] // This class specifically tests a case when AllowPrivate is NOT set to true.
        internal record InternalClassWithPublicMembers
        {
            [Key(0)]
            public int PublicProperty { get; set; }
        }

#if !UNITY_2018_3_OR_NEWER

        [Fact]
        public void RoundtripGenericClass_StandardResolverFallsBackOnInitProperty_Map()
        {
            var person = new GenericPersonMap<int> { Name = "bob" };
            var options = StandardResolver.Options;
            var deserialized = Roundtrip(person, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripGenericClass_StandardResolverFallsBackOnInitProperty()
        {
            var person = new GenericPerson<int> { Name = "bob" };
            var options = StandardResolver.Options;
            var deserialized = Roundtrip(person, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripNonGenericClass_StandardResolverWorksWithInitPropertySetter()
        {
            var person = new Person { Name = "bob" };
            var options = StandardResolver.Options;
            byte[] msgpack = MessagePackSerializer.Serialize(person, options);
            var deserialized = MessagePackSerializer.Deserialize<Person>(msgpack, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripGenericClass_StandardResolverWorksWithDeserializingCtor()
        {
            var person = new GenericPersonWithCtor<int>("bob");
            var options = StandardResolver.Options;
            byte[] msgpack = MessagePackSerializer.Serialize(person, options);
            var deserialized = MessagePackSerializer.Deserialize<GenericPersonWithCtor<int>>(msgpack, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripGenericClass_AllowPrivateStandardResolver()
        {
            var person = new GenericPerson<int> { Name = "bob" };
            var options = StandardResolverAllowPrivate.Options;
            var deserialized = Roundtrip(person, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripGenericClass_AllowPrivateStandardResolver_Map()
        {
            var person = new GenericPersonMap<int> { Name = "bob" };
            var options = StandardResolverAllowPrivate.Options;
            var deserialized = Roundtrip(person, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        [Fact]
        public void RoundtripGenericClass_AllowPrivateStandardResolver_Struct()
        {
            var person = new GenericPersonStruct<int> { Name = "bob" };
            var options = StandardResolverAllowPrivate.Options;
            var deserialized = Roundtrip(person, options);
            Assert.Equal(person.Name, deserialized.Name);
        }

        /// <summary>
        /// Verifies that SkipClrVisibilityChecks discovers the private members of a base class.
        /// </summary>
        /// <remarks>
        /// This test is only particularly effective when run in isolation, since other tests may have already
        /// triggered the skip checks attribute for the test assembly itself, which nullifies the goal of this test.
        /// </remarks>
        [Fact]
        public void SkipClrVisibilityTracing()
        {
            InternalTestWithoutMembersClass result = Roundtrip(new InternalTestWithoutMembersClass { BaseValue = 1 }, MessagePackSerializerOptions.Standard);
            Assert.Equal(1, result.BaseValue);
        }

        private T Roundtrip<T>(T value, MessagePackSerializerOptions options)
        {
            byte[] msgpack = MessagePackSerializer.Serialize(value, options);
            logger.WriteLine(MessagePackSerializer.ConvertToJson(msgpack));
            return MessagePackSerializer.Deserialize<T>(msgpack, options);
        }

#endif

        [MessagePackObject(true)]
        public class DefaultValueStringKeyClassWithoutExplicitConstructor
        {
            public const int Prop1Constant = 11;
            public const int Prop2Constant = 45;

            public int Prop1 { get; set; } = Prop1Constant;

            public int Prop2 { get; set; } = Prop2Constant;
        }

        [MessagePackObject(true)]
        public class DefaultValueStringKeyClassWithExplicitConstructor
        {
            public const int Prop2Constant = 1419;

            public int Prop1 { get; set; }

            public int Prop2 { get; set; }

            public DefaultValueStringKeyClassWithExplicitConstructor(int prop1)
            {
                Prop1 = prop1;
                Prop2 = Prop2Constant;
            }
        }

        [MessagePackObject(true)]
        public struct DefaultValueStringKeyStructWithExplicitConstructor
        {
            public const int Prop2Constant = 198;

            public int Prop1 { get; set; }

            public int Prop2 { get; set; }

            public DefaultValueStringKeyStructWithExplicitConstructor(int prop1)
            {
                Prop1 = prop1;
                Prop2 = Prop2Constant;
            }
        }

        [MessagePackObject]
        public class DefaultValueIntKeyClassWithoutExplicitConstructor
        {
            public const int Prop1Constant = 33;
            public const int Prop2Constant = -4;

            [Key(0)]
            public int Prop1 { get; set; } = Prop1Constant;

            [Key(1)]
            public int Prop2 { get; set; } = Prop2Constant;
        }

        [MessagePackObject]
        public class DefaultValueIntKeyClassWithExplicitConstructor
        {
            public const int Prop2Constant = -109;

            [Key(0)]
            public int Prop1 { get; set; }

            [Key(1)]
            public int Prop2 { get; set; }

            public DefaultValueIntKeyClassWithExplicitConstructor(int prop1)
            {
                Prop1 = prop1;
                Prop2 = Prop2Constant;
            }
        }

        [MessagePackObject]
        public struct DefaultValueIntKeyStructWithExplicitConstructor
        {
            public const int Prop2Constant = 31;

            [Key(0)]
            public int Prop1 { get; set; }

            [Key(1)]
            public int Prop2 { get; set; }

            public DefaultValueIntKeyStructWithExplicitConstructor(int prop1)
            {
                Prop1 = prop1;
                Prop2 = Prop2Constant;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualProperty
        {
            [DataMember]
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesProperty : BaseClassWithVirtualProperty
        {
            [DataMember]
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualPropertyDataMemberOnOverrideOnly
        {
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesPropertyDataMemberOnOverrideOnly : BaseClassWithVirtualPropertyDataMemberOnOverrideOnly
        {
            [DataMember]
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualPropertyDataMemberOnVirtualOnly
        {
            [DataMember]
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesPropertyDataMemberOnVirtualOnly : BaseClassWithVirtualPropertyDataMemberOnVirtualOnly
        {
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualPropertyOrdinalKey
        {
            [DataMember(Order = 0)]
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesPropertyOrdinalKey : BaseClassWithVirtualPropertyOrdinalKey
        {
            [DataMember(Order = 0)]
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualPropertyDataMemberOnOverrideOnlyOrdinalKey
        {
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesPropertyDataMemberOnOverrideOnlyOrdinalKey : BaseClassWithVirtualPropertyDataMemberOnOverrideOnlyOrdinalKey
        {
            [DataMember(Order = 0)]
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class BaseClassWithVirtualPropertyDataMemberOnVirtualOnlyOrdinalKey
        {
            [DataMember(Order = 0)]
            public virtual int VirtualProperty { get; set; }
        }

        [DataContract]
        public class DerivedClassThatOverridesPropertyDataMemberOnVirtualOnlyOrdinalKey : BaseClassWithVirtualPropertyDataMemberOnVirtualOnlyOrdinalKey
        {
            public override int VirtualProperty
            {
                get => base.VirtualProperty;
                set => base.VirtualProperty = value;
            }
        }

        [DataContract]
        public class TwoProperties
        {
            [DataMember]
            public string Prop1 { get; set; } = "Uninitialized";

            [DataMember]
            public string Prop2 { get; set; } = "Uninitialized";
        }

        [DataContract]
        public class TwoPropertiesOrdinalKey
        {
            [DataMember(Order = 0)]
            public string Prop1 { get; set; } = "Uninitialized";

            [DataMember(Order = 1)]
            public string Prop2 { get; set; } = "Uninitialized";
        }

        [MessagePackObject]
        public class TestMessageWithReadOnlyField
        {
            [Key(0)]
            public int Field1 = 1;

            [Key(1)]
            public readonly int Field2 = 2;

            [Key(2)]
            public int Field3 = 3;
        }

        [MessagePackObject]
        public class TestMessageWithReadWriteFields
        {
            [Key(0)]
            public int Property1 = 1;

            [Key(1)]
            public int Property2 = 2;

            [Key(2)]
            public int Property3 = 3;
        }

        [MessagePackObject]
        public class TestMessageWithReadOnlyProperty
        {
            [Key(0)]
            public int Property1 { get; set; } = 1;

            [Key(1)]
            public int Property2 => 2;

            [Key(2)]
            public int Property3 { get; set; } = 3;
        }

        [DataContract]
        public class BaseClass
        {
            [DataMember]
            private int baseClassField;

            public int BaseClassFieldAccessor
            {
                get => this.baseClassField;
                set => this.baseClassField = value;
            }

            [DataMember]
#pragma warning disable SA1300 // Element should begin with upper-case letter
            private int baseClassProperty { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

            public int BaseClassProperty
            {
                get => this.baseClassProperty;
                set => this.baseClassProperty = value;
            }
        }

        [DataContract]
        public class DerivedClass : BaseClass
        {
            [DataMember]
            public string Name { get; set; }
        }

#if !UNITY_2018_3_OR_NEWER
        [MessagePackObject]
        public class Person
        {
            [Key(0)]
            public string Name { get; init; }
        }

        [MessagePackObject]
        public class GenericPerson<T>
        {
            [Key(0)]
            public string Name { get; init; }
        }

        [MessagePackObject]
        public struct GenericPersonStruct<T>
        {
            [Key(0)]
            public string Name { get; init; }
        }

        [MessagePackObject]
        public class GenericPersonMap<T>
        {
            [Key("Name")]
            public string Name { get; init; }
        }

        [MessagePackObject]
        public class GenericPersonWithCtor<T>
        {
            [SerializationConstructor]
            public GenericPersonWithCtor(string name) => this.Name = name;

            [Key(0)]
            public string Name { get; init; }
        }
#endif

        [MessagePackObject(true)]
        public class ClassWithPropertySetterAndDummyCtor
        {
            public int MyProperty { get; set; }

            public ClassWithPropertySetterAndDummyCtor(int myProperty)
            {
                // This constructor intentionally left blank.
            }
        }

        [MessagePackObject(AllowPrivate = true, SuppressSourceGeneration = true)]
        public class InternalTestWithoutMembersClass : InternalBaseTestClass
        {
        }

        [MessagePackObject(AllowPrivate = true, SuppressSourceGeneration = true)]
        public class InternalBaseTestClass
        {
            [Key(0)]
            private int baseValue;

            [IgnoreMember]
            public int BaseValue { get => baseValue; set => baseValue = value; }
        }
    }
}

#endif
