using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Data;

namespace Tests.DataContainers
{
    public class DataListTests
    {
        [Test]
        public void TestTryGetValue()
        {
            DataList list = new DataList("a", "b", "c");
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
            DataList test = new DataList();
            Assert.AreEqual(0, test.Count, "initialized new empty list");
            test.Add("a");
            Assert.AreEqual(1, test.Count, "added one entry");
            test = new DataList("a", "b", "c");
            Assert.AreEqual(3, test.Count, "initialized new list with 3 entries");
            test.Remove("c");
            Assert.AreEqual(2, test.Count, "removed one entry");
            test = new DataList("a", "b", "c", "d", "e", "f", new DataDictionary(), "h");
            Assert.AreEqual(8, test.Count, "initialized new list with 8 entries");
        }

        [Test]
        public void TestInsert()
        {
            DataList list = new DataList("a", "b", "c");
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
            DataList list = new DataList("a", "b", "c");
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
            DataList list = new DataList("a", "b", "c", "d", "e", "f");
            DataList output = list.GetRange(0, 3);
            Assert.IsTrue(CompareList(output, new DataList("a", "b", "c")), "get half from start");
            output = list.GetRange(3, 3);
            Assert.IsTrue(CompareList(output, new DataList("d", "e", "f")), "get half from end");
        }

        [Test]
        public void TestShallowClone()
        {
            DataList list = new DataList("a", "b", "c");
            DataList clone = list.ShallowClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            
            list = new DataList("a", "b", new DataList(1, 2, 3, 4));
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
            DataList list = new DataList("a", "b", "c");
            DataList clone = list.DeepClone();
            Assert.IsTrue(CompareList(clone, list), "cloned list did not match source list");
            
            list = new DataList("a", "b", new DataList(1, 2, 3, 4));
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
            DataList list = new DataList("a", "b", "c");
            DataToken[] tokens = list.ToArray();
            Assert.IsTrue(CompareListToArray(list, tokens), "list did not match array");
        }

        [Test]
        public void TestAdd()
        {
            DataList list = new DataList();
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
            DataList list = new DataList();
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
            Assert.IsTrue(new DataList("a").Contains("a"));
            Assert.IsTrue(new DataList("a", "b", "c").Contains("c"));
            Assert.IsTrue(new DataList("a","b", "c", "a", "b", "c").Contains("c"));
            Assert.IsFalse(new DataList("a", "b", "c").Contains("d"));
        }

        [Test]
        public void TestIndexOf()
        {
            DataList list = new DataList() { "a", "b", "c", "a", "b", "c"};
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
            DataList list = new DataList() { "a", "b", "c", "a", "b", "c"};
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
            DataList list = new DataList("a", "b", "c");
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
            DataList list = new DataList("a", "b", "c");
            list.RemoveAt(0);
            Assert.IsTrue(CompareList(list, new DataList("b", "c")));
            list.RemoveAt(1);
            Assert.IsTrue(CompareList(list, new DataList("b")));
        }

        [Test]
        public void TestRemoveRange()
        {
            DataList list = new DataList("a", "b", "c", "a", "b", "c");
            list.RemoveRange(0, 2);
            Assert.IsTrue(CompareList(list, new DataList("c", "a", "b", "c")));
            list.RemoveRange(3, 1);
            Assert.IsTrue(CompareList(list, new DataList("c", "a", "b")));
        }

        [Test]
        public void TestClear()
        {
            DataList list = new DataList("a", "b", "c");
            list.Clear();
            Assert.IsTrue(CompareList(list, new DataList()));
        }

        [Test]
        public void TestReverse()
        {
            DataList list = new DataList("a", "b", "c", "a", "b", "c");
            list.Reverse();
            Assert.IsTrue(CompareList(list, new DataList("c", "b", "a", "c", "b", "a")));
            list.Reverse();
            Assert.IsTrue(CompareList(list, new DataList("a", "b", "c", "a", "b", "c")));
        }

        [Test]
        public void TestReverseRange()
        {
            DataList list = new DataList("a", "b", "c", "a", "b", "c");
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
            DataList list = new DataList(5, 4, 7, 2, 1);
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(1, 2, 4, 5, 7)));
            
            list = new DataList((double)8.5f, (float)5.3f, (long)-999999, (ulong)9999999, -53, (uint)9, (short)-32, (ushort)8, (byte)5, (sbyte)6);
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList((long)-999999, -53, (short)-32, (byte)5, (float)5.3f, (sbyte)6,  (ushort)8, (double)8.5f, (uint)9, (ulong)9999999)));

            list = new DataList(new DataList("a", "b", "c"), new DataList("a", "b"));
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(new DataList("a", "b"), new DataList("a", "b", "c"))));

            list = new DataList(4, true, new DataToken(),new DataList("a"), "string");
            list.Sort();
            Assert.IsTrue(CompareList(list, new DataList(new DataToken(), true, 4, "string", new DataList("a"))));
        }
        [Test]
        public void TestSortRange()
        {
            DataList list = new DataList(5, 4, 7, 2, 1);
            list.Sort(0, 3);
            Assert.IsTrue(CompareList(list, new DataList(4, 5, 7, 2, 1)));

            list = new DataList((double)8.5f, (float)5.3f, (long)-999999, (ulong)9999999, -53, (uint)9, (short)-32, (ushort)8, (byte)5, (sbyte)6);
            list.Sort(0, 8);
            Assert.IsTrue(CompareList(list, new DataList((long)-999999, -53, (short)-32, (float)5.3f, (ushort)8, (double)8.5f, (uint)9, (ulong)9999999,(byte)5, (sbyte)6)));

            list = new DataList(new DataList("a", "b", "c"), new DataList("a", "b", "c", "d"), new DataList("a", "b"));
            list.Sort(1, 2);
            Assert.IsTrue(CompareList(list, new DataList( new DataList("a", "b", "c"), new DataList("a", "b"),new DataList("a", "b", "c", "d"))));

            list = new DataList(4, true, new DataToken(),new DataList("a"), "string");
            list.Sort(1, 2);
            Assert.IsTrue(CompareList(list, new DataList(4,  new DataToken(),true,new DataList("a"), "string")));
        }
        [Test]
        public void TestBinarySearch()
        {
            DataList list = new DataList() {0,  10, 20, 30, 40, 50, 60, 70, 80, 90, 100};
            //Should be valid
            Assert.AreEqual(1, list.BinarySearch(10), "value exists, should succeed");
            Assert.AreEqual(-7, list.BinarySearch(55));
            Assert.AreEqual(4, list.BinarySearch(2, 5, 40), "value exists inside range, should succeed");
            Assert.AreEqual(7, list.BinarySearch(5, 6, 70), "count at last entry, should succeed");
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