using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Data;

namespace Tests.DataContainers
{
    
    public class JsonDictionaryTests : MonoBehaviour
    {
        [Test]
        public void TestAdd()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"key1\":\"value1\", \"key2\":\"value2\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.AreEqual("value1", dictionary["key1"]);
            Assert.AreEqual("value2", dictionary["key2"]);
            Assert.AreEqual(2, dictionary.Count);
            Assert.AreEqual(2, dictionary.GetKeys().Count);
            Assert.AreEqual(2, dictionary.GetValues().Count);
            dictionary.Add("key3", "value3");
            Assert.AreEqual("value3", dictionary["key3"]);
            Assert.AreEqual(3, dictionary.Count);
            Assert.AreEqual(3, dictionary.GetKeys().Count);
            Assert.AreEqual(3, dictionary.GetValues().Count);
            dictionary.Add(new KeyValuePair<DataToken, DataToken>("key4", "value4"));
            Assert.AreEqual("value4", dictionary["key4"]);
            Assert.AreEqual(4, dictionary.Count);
            Assert.AreEqual(4, dictionary.GetKeys().Count);
            Assert.AreEqual(4, dictionary.GetValues().Count);
        }
        [Test]
        public void TestTryGetValue()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"a\", \"b\":\"b\", \"c\":\"c\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(dictionary.TryGetValue("a", out DataToken value));
            Assert.AreEqual("a", value);
            Assert.IsTrue(dictionary.TryGetValue("a", TokenType.String, out value));
            Assert.AreEqual("a", value);
            
            Assert.IsFalse(dictionary.TryGetValue("x", out value));
            Assert.AreEqual(DataError.KeyDoesNotExist, value);
            Assert.IsFalse(dictionary.TryGetValue("a", TokenType.Boolean, out value));
            Assert.AreEqual(DataError.TypeMismatch, value);
        }
        
        [Test]
        public void TestCount()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.AreEqual(0, dictionary.Count, "initialized new empty list");
            dictionary.SetValue("a", "a");
            Assert.AreEqual(1, dictionary.Count, "added one entry");
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"a\", \"b\":\"b\", \"c\":\"c\"}", out token));
            dictionary = token.DataDictionary;
            Assert.AreEqual(3, dictionary.Count, "initialized new list with 3 entries");
            dictionary.Remove("c");
            Assert.AreEqual(2, dictionary.Count, "removed one entry");
            dictionary = new DataDictionary() {["a"]="a", ["b"]="b", ["c"]="c", ["d"]="d", ["e"]="e", ["f"] = "f", ["g"]=new DataDictionary(), ["h"] = "h"};
            Assert.AreEqual(8, dictionary.Count, "initialized new list with 8 entries");
        }

        [Test]
        public void TestClear()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"a\", \"b\":\"b\", \"c\":\"c\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.AreEqual(3, dictionary.Count);
            dictionary.Clear();
            Assert.AreEqual(0, dictionary.Count);
        }

        [Test]
        public void TestGetKeys()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"a\", \"b\":\"b\", \"c\":\"c\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(CompareList(dictionary.GetKeys(), new DataList("a", "b", "c")));
            dictionary.SetValue("d", "d");
            Assert.IsTrue(dictionary.GetKeys().Contains("d"));
            dictionary.Remove("d");
            Assert.IsFalse(dictionary.GetKeys().Contains("d"));
            dictionary.Clear();
            Assert.IsFalse(dictionary.GetKeys().Contains("a"));
        }

        [Test]
        public void TestGetValues()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"a\", \"b\":\"b\", \"c\":\"c\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(CompareList(dictionary.GetValues(), new DataList("a", "b", "c")));
            dictionary.SetValue("d", "d");
            Assert.IsTrue(dictionary.GetValues().Contains("d"));
            dictionary.Remove("d");
            Assert.IsFalse(dictionary.GetValues().Contains("d"));
            dictionary.Clear();
            Assert.IsFalse(dictionary.GetValues().Contains("a"));
        }

        [Test]
        public void TestRemove()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"x\", \"b\":\"y\", \"c\":\"z\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(dictionary.Remove("b"));
            Assert.IsFalse(dictionary.ContainsKey("b"));
            Assert.IsFalse(dictionary.Remove("f"));

            Assert.IsTrue(dictionary.Remove("c", out DataToken value));
            Assert.AreEqual(value, "z");
            Assert.IsFalse(dictionary.Remove("c", out value));
            Assert.AreEqual(value, DataError.KeyDoesNotExist);
        }

        [Test]
        public void TestContainsKey()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"x\", \"b\":\"y\", \"c\":\"z\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(dictionary.ContainsKey("b"));
            dictionary.Remove("b");
            Assert.IsFalse(dictionary.ContainsKey("b"));
        }

        [Test]
        public void TestContainsValue()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("{\"a\":\"x\", \"b\":\"y\", \"c\":\"z\"}", out DataToken token));
            DataDictionary dictionary = token.DataDictionary;
            Assert.IsTrue(dictionary.ContainsValue("y"));
            dictionary.Remove("b");
            Assert.IsFalse(dictionary.ContainsValue("y"));
        }
        
        
        private bool CompareList(DataList a, DataList b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].TokenType == TokenType.DataList)
                {
                    if (b[i].TokenType != TokenType.DataList) return false;
                    if (!CompareList(a[i].DataList, b[i].DataList)) return false;
                }
                else
                {
                    if (a[i] != b[i]) return false;
                }
            }

            return true;
        }
    }
}