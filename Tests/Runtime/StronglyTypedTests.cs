using System.Collections.Generic;
using Chartboost.Generics;
using NUnit.Framework;

namespace Chartboost.Tests
{
    public class StronglyTypedTests
    {
        private sealed class Key : StronglyTyped<string>
        {
            public Key(string value) : base(value) { }
        }

        private sealed class OtherKey : StronglyTyped<string>
        {
            public OtherKey(string value) : base(value) { }
        }

        // HB-12127: equality was by reference, so keys rebuilt from native/JSON never matched.
        [Test]
        public void InstancesWithTheSameValueAreEqual()
        {
            Assert.AreEqual(new Key("a"), new Key("a"));
            Assert.AreEqual(new Key("a").GetHashCode(), new Key("a").GetHashCode());
        }

        [Test]
        public void DifferentValuesTypesOrNullAreNotEqual()
        {
            Assert.IsFalse(new Key("a").Equals(new Key("b")));
            Assert.IsFalse(new Key("a").Equals(new OtherKey("a")));
            Assert.IsFalse(new Key("a").Equals(null));
            Assert.IsFalse(new Key("a").Equals("a"));
        }

        [Test]
        public void NullValuesAreEqual()
            => Assert.IsTrue(new Key(null).Equals(new Key(null)));

        [Test]
        public void DictionaryFindsAnEqualKey()
        {
            var consents = new Dictionary<Key, string> { { new Key("IABTCF_TCString"), "tcf" } };

            Assert.IsTrue(consents.ContainsKey(new Key("IABTCF_TCString")));
        }
    }
}
