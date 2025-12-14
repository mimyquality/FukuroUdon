using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Data;

namespace Tests.DataContainers
{
    public class JsonListTests : MonoBehaviour
    {
        [Test]
        public void TestTryGetValue()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            Assert.IsTrue(list.TryGetValue(1, out DataToken value));
            Assert.AreEqual("b", value);
            Assert.IsTrue(list.TryGetValue(2, TokenType.String, out value));
            Assert.AreEqual("c", value);

            Assert.IsFalse(list.TryGetValue(-1, out value));
            Assert.AreEqual(DataError.IndexOutOfRange, value);
            Assert.IsFalse(list.TryGetValue(3, out value));
            Assert.AreEqual(DataError.IndexOutOfRange, value);
            Assert.IsFalse(list.TryGetValue(-1, TokenType.String, out value));
            Assert.AreEqual(DataError.IndexOutOfRange, value);
            Assert.IsFalse(list.TryGetValue(3, TokenType.String, out value));
            Assert.AreEqual(DataError.IndexOutOfRange, value);
            Assert.IsFalse(list.TryGetValue(0, TokenType.Boolean, out value));
            Assert.AreEqual(DataError.TypeMismatch, value);
        }

        [Test]
        public void TestCount()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[]", out DataToken token));
            DataList list = token.DataList;
            Assert.AreEqual(0, list.Count, "initialized new empty list");
            list.Add("a");
            Assert.AreEqual(1, list.Count, "added one entry");
            list = new DataList("a", "b", "c");
            Assert.AreEqual(3, list.Count, "initialized new list with 3 entries");
            list.Remove("c");
            Assert.AreEqual(2, list.Count, "removed one entry");
            list = new DataList("a", "b", "c", "d", "e", "f", new DataDictionary(), "h");
            Assert.AreEqual(8, list.Count, "initialized new list with 8 entries");
        }

        [Test]
        public void TestInsert()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.Insert(0, "inserted1");
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "a", "b", "c")));
            list.Insert(3, "inserted2");
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "a", "b", "inserted2", "c")));
            list.Insert(2, "inserted3");
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "a", "inserted3", "b", "inserted2", "c")));
            list.Insert(6, "inserted4");
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "a", "inserted3", "b", "inserted2", "c", "inserted4")));

        }

        [Test]
        public void TestInsertRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.InsertRange(0, new DataList("inserted1", "inserted2"));
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "inserted2", "a", "b", "c")));
            list.InsertRange(5, new DataList("inserted3", "inserted4"));
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "inserted2", "a", "b", "c", "inserted3", "inserted4")));
            list.InsertRange(2, new DataList("inserted5", "inserted6"));
            Assert.IsTrue(CompareList(list, new DataList("inserted1", "inserted2", "inserted5", "inserted6", "a", "b", "c", "inserted3", "inserted4")));
        }

        [Test]
        public void TestTryGetRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\", \"d\", \"e\", \"f\"]", out DataToken token));
            DataList list = token.DataList;
            DataList output = list.GetRange(0, 3);
            Assert.IsTrue(CompareList(output, new DataList("a", "b", "c")), "get half from start");
            output = list.GetRange(3, 3);
            Assert.IsTrue(CompareList(output, new DataList("d", "e", "f")), "get half from end");

        }
        [Test]
        public void TestShallowClone()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            DataList clone = list.ShallowClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", [1, 2, 3, 4]]", out token));
            list = token.DataList;
            clone = list.ShallowClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            clone.Add("c");
            Assert.IsFalse(CompareList(clone, list), "adding an item to the shallow cloned list affected the source list");

            clone = list.ShallowClone();
            clone[0] = 5;
            Assert.IsFalse(CompareList(clone, list), "modifying a value in the shallow cloned list affected the source list");
            list[2].DataList.Add(5);
            Assert.IsTrue(clone[2].DataList.Contains(5), "modifying a child of the shallow cloned list did not affect the source list");
        }

        [Test]
        public void TestDeepClone()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            DataList clone = list.DeepClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", [1, 2, 3, 4]]", out token));
            list = token.DataList;
            clone = list.DeepClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            clone.Add("c");
            Assert.IsFalse(CompareList(clone, list), "adding an item to the deep cloned list affected the source list");
            
            clone = list.DeepClone();
            clone[0] = 5;
            Assert.IsFalse(CompareList(clone, list), "modifying a value in the deep cloned list affected the source list");
            list[2].DataList.Add(5);
            Assert.IsFalse(clone[2].DataList.Contains(5), "modifying a child of the deep cloned list affected the source list");
        }

        [Test]
        public void TestToArray()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            DataToken[] tokens = list.ToArray();
            Assert.IsTrue(CompareListToArray(list, tokens), "list did not match array");
        }

        [Test]
        public void TestAdd()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[]", out DataToken token));
            DataList list = token.DataList;
            list.Add("a");
            Assert.IsTrue(CompareList(list, new DataList("a")));
            list.Add("b");
            Assert.IsTrue(CompareList(list, new DataList("a", "b")));
            list.Add("c");
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c")));
        }

        [Test]
        public void TestAddRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[]", out DataToken token));
            DataList list = token.DataList;
            list.AddRange(new DataList("a", "b"));
            Assert.IsTrue(CompareList(list, new DataList("a", "b")));
            list.AddRange(new DataList("c", "d"));
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c", "d")));
            list.AddRange(new DataList("e", "f"));
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c", "d", "e", "f")));
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\"]", out DataToken token));
            DataList list = token.DataList;
            Assert.IsTrue(list.Contains("a"));
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out token));
            list = token.DataList;
            Assert.IsTrue(list.Contains("c"));
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\",\"a\",\"b\",\"c\"]", out token));
            list = token.DataList;
            Assert.IsTrue(list.Contains("c"));
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out token));
            list = token.DataList;
            Assert.IsFalse(list.Contains("d"));
        }

        [Test]
        public void TestIndexOf()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\",\"a\",\"b\",\"c\"]", out DataToken token));
            DataList list = token.DataList;
            //Should be valid
            Assert.AreEqual(0, list.IndexOf("a"), "value exists, should succeed");
            Assert.AreEqual(3, list.IndexOf("a", 2), "value exists inside range, should succeed");
            Assert.AreEqual(3, list.IndexOf("a", 1, 5), "count at last entry, should succeed");
            Assert.AreEqual(5, list.IndexOf("c", 5), "start index at last entry, should succeed");
            //Should be invalid
            Assert.AreEqual(-1, list.IndexOf("f"), "value does not exist, should fail");
        }

        [Test]
        public void TestLastIndexOf()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\",\"b\",\"c\",\"a\",\"b\",\"c\"]", out DataToken token));
            DataList list = token.DataList;
            //Should be valid
            Assert.AreEqual(3, list.LastIndexOf("a"), "value exists, should succeed");
            Assert.AreEqual(2, list.LastIndexOf("c", 2), "value exists inside range, should succeed");
            Assert.AreEqual(3, list.LastIndexOf("a", 5, 3), "value exists inside range, should succeed");
            Assert.AreEqual(0, list.LastIndexOf("a", 2, 3), "count at first entry, should succeed");
            Assert.AreEqual(0, list.LastIndexOf("a", 0), "start index at first entry, should succeed");
            //Should be invalid
            Assert.AreEqual(-1, list.LastIndexOf("f"), "value does not exist, should fail");
        }

        [Test]
        public void TestRemove()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            Assert.IsTrue(list.Remove("b"));
            Assert.IsTrue(CompareList(list, new DataList("a", "c")));
            list = new DataList("a", "b", "c", "a", "b", "c");
            Assert.IsTrue(list.Remove("a"));
            Assert.IsTrue(CompareList(list, new DataList("b", "c", "a", "b", "c")));
            Assert.IsTrue(list.Remove("a"));
            Assert.IsTrue(CompareList(list, new DataList("b", "c", "b", "c")));

            Assert.IsFalse(list.Remove("a"));
            Assert.IsTrue(CompareList(list, new DataList("b", "c", "b", "c")));
        }

        [Test]
        public void TestRemoveAt()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.RemoveAt(0);
            Assert.IsTrue(CompareList(list, new DataList("b", "c")));
            list.RemoveAt(1);
            Assert.IsTrue(CompareList(list, new DataList("b")));
        }

        [Test]
        public void TestRemoveRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\",\"b\",\"c\",\"a\",\"b\",\"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.RemoveRange(0, 2);
            Assert.IsTrue(CompareList(list, new DataList("c", "a", "b", "c")));
            list.RemoveRange(3, 1);
            Assert.IsTrue(CompareList(list, new DataList("c", "a", "b")));
        }

        [Test]
        public void TestClear()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\", \"b\", \"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.Clear();
            Assert.IsTrue(CompareList(list, new DataList()));
        }

        [Test]
        public void TestReverse()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\",\"b\",\"c\",\"a\",\"b\",\"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.Reverse();
            Assert.IsTrue(CompareList(list, new DataList("c", "b", "a", "c", "b", "a")));
            list.Reverse();
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c", "a", "b", "c")));
        }

        [Test]
        public void TestReverseRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[\"a\",\"b\",\"c\",\"a\",\"b\",\"c\"]", out DataToken token));
            DataList list = token.DataList;
            list.Reverse(0, 3);
            Assert.IsTrue(CompareList(list, new DataList("c", "b", "a", "a", "b", "c")));
            list.Reverse(3, 3);
            Assert.IsTrue(CompareList(list, new DataList("c", "b", "a", "c", "b", "a")));
            list.Reverse(0, 6);
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c", "a", "b", "c")));
        }

        [Test]
        public void TestSort()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[5, 4, 7, 2, 1]", out DataToken token));
            DataList list = token.DataList;
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(1d, 2d, 4d, 5d, 7d)));

            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[8.5, 5.3, -999999, 9999999, -53, 9, -32, 8, 5, 6]", out token)); 
            list = token.DataList;
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(-999999d, -53d, -32d, 5d, 5.3d, 6d, 8d, 8.5d, 9d, 9999999d)));

            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[[\"a\", \"b\", \"c\"], [\"a\",\"b\"]]", out token)); 
            list = token.DataList;
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(new DataList("a", "b"), new DataList("a", "b", "c"))));
            
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[4, true, null, [\"a\"], \"string\"]", out token)); 
            list = token.DataList;
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(new DataToken(), true, 4d, "string", new DataList("a"))));
        }

        [Test]
        public void TestSortRange()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[5, 4, 7, 2, 1]", out DataToken token));
            DataList list = token.DataList;
            list.Sort(0, 3);
            Assert.IsTrue(CompareList(list, new DataList(4d, 5d, 7d, 2d, 1d)));

            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[8.5, 5.3, -999999, 9999999, -53, 9, -32, 8, 5, 6]", out token)); 
            list = token.DataList;
            list.Sort(0, 8);
            Assert.IsTrue(CompareList(list, new DataList(-999999d, -53d, -32d, 5.3d, 8d, 8.5d, 9d, 9999999d, 5d, 6d)));

            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[[\"a\", \"b\", \"c\"], [\"a\",\"b\"], [\"a\", \"b\", \"c\", \"d\"]]", out token)); 
            list = token.DataList;
            list.Sort(1, 2);
            Assert.IsTrue(CompareList(list, new DataList(new DataList("a", "b", "c"), new DataList("a", "b"), new DataList("a", "b", "c", "d"))));

            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[4, true, null, [\"a\"], \"string\"]", out token)); 
            list = token.DataList;
            list.Sort(1, 2);
            Assert.IsTrue(CompareList(list, new DataList(4d, new DataToken(), true, new DataList("a"), "string")));
        }

        [Test]
        public void TestBinarySearch()
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson("[0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100]", out DataToken token));
            DataList list = token.DataList;
            //Should be valid
            Assert.AreEqual(1, list.BinarySearch(10d), "value exists, should succeed");
            Assert.AreEqual(-7, list.BinarySearch(55d));
            Assert.AreEqual(4, list.BinarySearch(2, 5, 40d), "value exists inside range, should succeed");
            Assert.AreEqual(7, list.BinarySearch(5, 6, 70d), "count at last entry, should succeed");
            //Should be invalid
            Assert.AreEqual(-12, list.BinarySearch("f"), "value does not exist, should fail");
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

        private bool CompareListToArray(DataList a, DataToken[] b)
        {
            if (a.Count != b.Length) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
        }
    }
}
