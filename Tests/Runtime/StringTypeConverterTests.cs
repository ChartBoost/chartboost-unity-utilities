using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Chartboost.Generics;
using Chartboost.Json;
using Chartboost.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Chartboost.Tests
{
    public class StringTypeConverterTests : DebugLogLevelFixture
    {
        [TypeConverter(typeof(StringTypeConverter<Key>))]
        private sealed class Key : StronglyTyped<string>
        {
            public Key(string value) : base(value) { }
        }

        private readonly StringTypeConverter<Key> _keys = new();

        [Test]
        public void ConvertsOnlyFromAndToStrings()
        {
            Assert.IsTrue(_keys.CanConvertFrom(null, typeof(string)));
            Assert.IsFalse(_keys.CanConvertFrom(null, typeof(int)));
            Assert.IsTrue(_keys.CanConvertTo(null, typeof(string)));
            Assert.IsFalse(_keys.CanConvertTo(null, typeof(int)));
        }

        [Test]
        public void AStringBecomesAStronglyTypedValue()
            => Assert.AreEqual(new Key("abc"), _keys.ConvertFrom(null, null, "abc"));

        [Test]
        public void NullStaysNull()
            => Assert.IsNull(_keys.ConvertFrom(null, null, null));

        [Test]
        public void ANonStringSourceIsRejected()
            => Assert.Throws<NotSupportedException>(() => _keys.ConvertFrom(null, null, 42));

        [Test]
        public void AValueConvertsBackToItsString()
            => Assert.AreEqual("abc", _keys.ConvertTo(null, null, new Key("abc"), typeof(string)));

        [Test]
        public void ANonStringDestinationIsRejected()
            => Assert.Throws<NotSupportedException>(() => _keys.ConvertTo(null, null, new Key("abc"), typeof(int)));

        [Test]
        public void OtherTypesUseTheirOwnConverter()
            => Assert.AreEqual(42, new StringTypeConverter<int>().ConvertFrom(null, null, "42"));

        // How the converter is used: Newtonsoft turns dictionary keys into strongly typed keys.
        [Test]
        public void JsonDictionaryKeysRoundTrip()
        {
            var json = JsonTools.SerializeObject(new Dictionary<Key, int> { { new Key("a"), 1 } });
            var parsed = JsonTools.DeserializeObject<Dictionary<Key, int>>(json);

            Assert.AreEqual("{\"a\":1}", json);
            Assert.AreEqual(1, parsed[new Key("a")]);
        }

        [Test]
        public void ANullValueHasNoString()
        {
            var empty = new Key(null);

            Assert.IsNull(empty.ToString());
            Assert.IsNull((string)empty);
            Assert.IsNull((string)(Key)null);
        }

        [Test]
        public void SerializingNullWarnsAndReturnsEmpty()
        {
            LogAssert.Expect(LogType.Warning, new Regex("SerializeObject string value cannot be null"));

            Assert.AreEqual(string.Empty, JsonTools.SerializeObject<Key>(null));
        }
    }
}
