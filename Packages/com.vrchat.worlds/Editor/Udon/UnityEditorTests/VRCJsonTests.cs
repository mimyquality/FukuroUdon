using System;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Data;
using Random = UnityEngine.Random;

namespace Tests.DataContainers
{
    public class VRCJsonTests : MonoBehaviour
    {
        [Test]
        public void TestEmpty()
        {
            SetAndGetJson("empty", new DataToken());
        }

        [Test]
        public void TestBool()
        {
            SetAndGetJson("bool true", new DataToken(true));
            SetAndGetJson("bool false", new DataToken(false));
        }
        [Test]
        public void TestByte()
        {
            SetAndGetJson("byte number", (byte)4);
            SetAndGetJson("max byte number", byte.MaxValue);
            SetAndGetJson("min byte number", byte.MinValue);
        }
        [Test]
        public void TestSByte()
        {
            SetAndGetJson("sbyte number", (sbyte)4);
            SetAndGetJson("max sbyte number", sbyte.MaxValue);
            SetAndGetJson("min sbyte number", sbyte.MinValue);
        }
        [Test]
        public void TestShort()
        {
            SetAndGetJson("short number", (short)4);
            SetAndGetJson("max short number", short.MaxValue);
            SetAndGetJson("min short number", short.MinValue);
        }
        [Test]
        public void TestUShort()
        {
            SetAndGetJson("ushort number", (ushort)4);
            SetAndGetJson("max ushort number", ushort.MaxValue);
            SetAndGetJson("min ushort number", ushort.MinValue);
        }
        [Test]
        public void TestInt()
        {
            SetAndGetJson("int number", (int)4);
            SetAndGetJson("max int number", int.MaxValue);
            SetAndGetJson("min int number", int.MinValue);
        }
        [Test]
        public void TestUInt()
        {
            SetAndGetJson("uint number", (uint)4);
            SetAndGetJson("max uint number", uint.MaxValue);
            SetAndGetJson("min uint number", uint.MinValue);
        }
        [Test]
        public void TestLong()
        {
            SetAndGetJson("long number", (long)4);
            SetAndGetJson("max long number", long.MaxValue);
            SetAndGetJson("min long number", long.MinValue);
        }
        [Test]
        public void TestULong()
        {
            SetAndGetJson("ulong number", (ulong)4);
            SetAndGetJson("max ulong number", ulong.MaxValue);
            SetAndGetJson("min ulong number", ulong.MinValue);
        }
        [Test]
        public void TestFloat()
        {
            //SetAndGetJson("float number", 0.123f, false);
            SetAndGetJson( "1.23e-2)",1.23e-2f); 
            SetAndGetJson( "1.234e-5",1.234e-5f);
            SetAndGetJson( "1.2345E-10",1.2345E-10f);
            SetAndGetJson( "1.23456E-20",1.23456E-20f);
            SetAndGetJson( "5E-20",5E-20f);
            SetAndGetJson( "1.23E+2",1.23E+2f);
            SetAndGetJson( "1.234e5",1.234e5f);
            SetAndGetJson( "1.2345E10",1.2345E10f);
            SetAndGetJson( "-7.576E-05",-7.576E-05f);
            SetAndGetJson( "1.23456e20",1.23456e20f);
            SetAndGetJson( "5e+20",5e+20f);
            SetAndGetJson( "5e-200",5e-200f);
            SetAndGetJson( "9.1093822E-31",9.1093822E-31f);
            SetAndGetJson( "5.9736e24",5.9736e24f);
            SetAndGetJson("90.00001 float", 90.00001f);
            SetAndGetJson("max float number / 1000", float.MaxValue /1000);
            SetAndGetJson("max float number / 100", float.MaxValue /100);
            SetAndGetJson("max float number / 10", float.MaxValue /10);
            SetAndGetJson("max float number", float.MaxValue);
            SetAndGetJson("min float number", float.MinValue);
            SetAndGetJson("Epsilon float number", float.Epsilon);
        }
        [Test]
        public void TestDouble()
        {
            SetAndGetJson( "1",1); 
            SetAndGetJson( "1.23e-2",1.23e-2); 
            SetAndGetJson( "1.234e-5",1.234e-5);
            SetAndGetJson( "1.2345E-10",1.2345E-10);
            SetAndGetJson( "1.23456E-20",1.23456E-20);
            SetAndGetJson( "5E-20",5E-20);
            SetAndGetJson( "1.23E+2",1.23E+2);
            SetAndGetJson( "1.234e5",1.234e5);
            SetAndGetJson( "1.2345E10",1.2345E10);
            SetAndGetJson( "-7.576E-05",-7.576E-05);
            SetAndGetJson( "1.23456e20",1.23456e20);
            SetAndGetJson( "5e+20",5e+20);
            SetAndGetJson( "5e-200",5e-200);
            SetAndGetJson( "9.1093822E-31",9.1093822E-31);
            SetAndGetJson( "5.9736e24",5.9736e24);
            SetAndGetJson("max float number as double", (double)float.MaxValue);
            SetAndGetJson("min float number as double", (double)float.MinValue);
            SetAndGetJson("double MaxValue", double.MaxValue);
            SetAndGetJson( "double MinValue",double.MinValue);
            SetAndGetJson( "double Epsilon",double.Epsilon);
        }
        
        [Test]
        public void TestString()
        {
            SetAndGetJson("backspace", new DataToken("\b"));
            SetAndGetJson("form feed", new DataToken("\f"));
            SetAndGetJson("newline", new DataToken("\n"));
            SetAndGetJson("carriage return", new DataToken("\r"));
            SetAndGetJson("tab", new DataToken("\t"));
            SetAndGetJson("quotes", new DataToken("\""));
            SetAndGetJson("victory hand", new DataToken("✌"));
            SetAndGetJson("mandarin", new DataToken("䉟"));
            SetAndGetJson("greater than", new DataToken(">"));
        }
        [Test]
        public void RecursiveSerialization()
        {
            DataList a = new DataList();
            a.Add(1);
            a.Add(2);
            a.Add(3);

            DataList b = new DataList();
            b.Add(1);
            b.Add(2);
            b.Add(3);
            
            b.Add(a);
            a.Add(b);

            Assert.IsFalse(VRCJson.TrySerializeToJson(a, JsonExportType.Beautify, out DataToken result), "should fail serialization because it's recursive");

            b.Remove(a);
            
            Assert.IsTrue(VRCJson.TrySerializeToJson(a, JsonExportType.Beautify, out result), "Should succeed serialization because it's not recursive");
            
            a.Add(b);
            
            Assert.IsTrue(VRCJson.TrySerializeToJson(a, JsonExportType.Beautify, out result), "Should succeed serialization because it's not recursive, even if it contains multiple copies of the same container");
            
        }
        
        private void SetAndGetJson(string title, DataToken inToken)
        {
            SerializeDictionary(title + " through serialization", inToken);
            SerializeList(title + " through serialization", inToken);
        }
        private void SerializeDictionary(string title, DataToken inToken)
        {
            DataDictionary dataDictionary = new DataDictionary();
            dataDictionary.SetValue("key", inToken);
            
            Assert.IsTrue(VRCJson.TrySerializeToJson(dataDictionary, JsonExportType.Minify, out DataToken serialized), $"{title} failed to serialize to JSON with error ({serialized})");
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(serialized.String, out DataToken deserialized), $"failed to deserialize JSON {serialized.ToString()} with error {deserialized} ");
            Assert.IsTrue(deserialized.DataDictionary.TryGetValue("key", out DataToken outToken), $"{title} failed to get value with error {outToken}");
            CompareTokens(title, inToken, outToken);
        }
        private void SerializeList(string title, DataToken inToken)
        {
            DataList dataList = new DataList();
            dataList.Add(inToken);
            
            Assert.IsTrue(VRCJson.TrySerializeToJson(dataList, JsonExportType.Minify, out DataToken serialized), $"{title} failed to serialize to JSON ({serialized})");
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(serialized.String, out DataToken deserialized), $"failed to deserialize JSON {serialized.ToString()} with error {deserialized} ");
            Assert.IsTrue(deserialized.DataList.TryGetValue(0, out DataToken outToken), $"{title} Failed to get value with error {outToken}");
            CompareTokens(title, inToken, outToken);
        }

        private void CompareTokens(string title, DataToken a, DataToken b)
        {
            
            if (a.TokenType == TokenType.Float)
            {
                Assert.IsTrue(Mathf.Approximately(a.Float, Convert.ToSingle(b.Double)),  $"{title} Input ({a.Float.ToString("G")}) and output ({b.Double.ToString("G")}) tokens were not the same");
            }
            else if (a.IsNumber)
            {
                Assert.IsTrue(Approximately(a.Double, b.Double),  $"{title} Input ({a.Double.ToString("G")}) and output ({b.Number.ToString("G")}) tokens were not the same");
            }
            else
            {
                Assert.AreEqual(expected: a, b,  $"{title} Input ({a}) and output ({b}) tokens were not the same");
            }
        }
        private bool Approximately(double a, double b)
        {
            if (Math.Sign(a) != Math.Sign(b)) return false;
            a = Math.Abs(a);
            b = Math.Abs(b);
            double max = Math.Max(a, b);
            return a - b < Math.Max(max / 100000000000, 0.1f);
        }

        [Test]
        public void TestUnsupportedNumbers()
        {
            ShouldFailSerialization(new DataList() {float.NaN}, DataError.ValueUnsupported);
            ShouldFailSerialization(new DataList() {float.NegativeInfinity}, DataError.ValueUnsupported);
            ShouldFailSerialization(new DataList() {float.PositiveInfinity }, DataError.ValueUnsupported);
            ShouldFailSerialization(new DataList() {double.NaN}, DataError.ValueUnsupported);
            ShouldFailSerialization(new DataList() {double.NegativeInfinity}, DataError.ValueUnsupported);
            ShouldFailSerialization(new DataList() {double.PositiveInfinity}, DataError.ValueUnsupported);
        }

        [Test]
        public void TestUnsupportedReferences()
        {
            ShouldFailSerialization(new DataList() {new DataToken(new bool[]{false, true, false})}, DataError.TypeUnsupported);
            ShouldFailSerialization(new DataList() {new DataToken(DateTime.Now)}, DataError.TypeUnsupported);
        }

        [Test]
        public void TestUnsupportedDictionaryKeys()
        {
            ShouldFailSerialization(new DataDictionary() { [312] = "value" }, DataError.TypeUnsupported);
            ShouldFailSerialization(new DataDictionary() { [true] = "value" }, DataError.TypeUnsupported);
            ShouldFailSerialization(new DataDictionary() { [false] = "value" }, DataError.TypeUnsupported);
            ShouldFailSerialization(new DataDictionary() { [5.432f] = "value" }, DataError.TypeUnsupported);
            ShouldFailSerialization(new DataDictionary() { [new DataList()] = "value" }, DataError.TypeUnsupported);
        }

        private void ShouldFailSerialization(DataToken token, DataError expectedError)
        {
            Assert.IsFalse(VRCJson.TrySerializeToJson(token, JsonExportType.Minify, out DataToken result), "Should fail to serialize");
            Assert.AreEqual(result, expectedError, $"Resulting error should be {expectedError}");
        }
        
        [Test]
        public void TestGoodJsonArrays()
        {
            ValidateJsonList("[]", new DataToken[]{});
            ValidateJsonList("[\"value\"]",  new DataToken[]{"value"});
            ValidateJsonList("[\"value1\", \"value2\",\"value3\"]",  new DataToken[]{"value1", "value2", "value3"});
            ValidateJsonList("[1]",  new DataToken[]{1});
            ValidateJsonList("[1, 2, 3]",  new DataToken[]{1, 2, 3});
            ValidateJsonList("[1, 2, 3,]", new DataToken[]{1, 2, 3});
        }
        
        [Test]
        public void TestGoodJsonObjects()
        {
            ValidateJsonObject("{}", new DataToken[]{}, new DataToken[] {});
            ValidateJsonObject("{\"key\":\"value\"}", new DataToken[]{"key"}, new DataToken[] {"value"});
            ValidateJsonObject("{\"key\": \"value\",}", new DataToken[] {"key"}, new DataToken[] {"value"});
        }

        [Test]
        public void TestGoodJsonTokens()
        {
            ValidateJsonToken("[\"\\u0020\"]", " ");
            ValidateJsonToken("[\"\\u00A9\"]", "©");
            ValidateJsonToken("[\"\\u00E9\"]", "é");
            ValidateJsonToken("[\"\\u3042\"]", "あ");
            ValidateJsonToken("[true]", true);
            ValidateJsonToken("[false]", false);
            ValidateJsonToken("[null]", new DataToken()); 
            ValidateJsonToken( $"[{double.MaxValue.ToString(CultureInfo.InvariantCulture).Replace("+", "")}]", double.MaxValue); 
            ValidateJsonToken( $"[{double.MinValue.ToString(CultureInfo.InvariantCulture).Replace("+", "")}]", double.MinValue); 
            ValidateJsonToken( $"[{double.Epsilon.ToString(CultureInfo.InvariantCulture).Replace("+", "")}]", double.Epsilon); 
        }
        
        private void ValidateJsonList(string input, DataToken[] expectedTokens)
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(input, out DataToken list), $"Attempting to deserialize {input}");
            Assert.AreEqual(list.TokenType, TokenType.DataList, $"Comparing type of {input}");
            Assert.AreEqual(list.DataList.Count, expectedTokens.Length, $"Comparing count of {input}");
            for (int i = 0; i < list.DataList.Count; i++)
            {
                Assert.IsTrue(list.DataList.TryGetValue(i, out DataToken value), $"Attempting to get value {i} from {input}, hit {value.Error}");
                CompareTokens($"Comparing token {i} in {input}", value, expectedTokens[i]);
            }
        }
        private void ValidateJsonObject(string input, DataToken[] expectedKeys, DataToken[] expectedValues)
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(input, out DataToken dictionary), $"Attempting to deserialize {input}");
            Assert.AreEqual(dictionary.TokenType, TokenType.DataDictionary, $"Comparing type of {input}");
            Assert.AreEqual(dictionary.DataDictionary.Count, expectedValues.Length, $"Comparing count of {input}");
            DataList keys = dictionary.DataDictionary.GetKeys();
            for (int i = 0; i < dictionary.DataDictionary.Count; i++)
            {
                CompareTokens($"comparing key {i}", expectedKeys[i], keys[i]);
                Assert.IsTrue(dictionary.DataDictionary.TryGetValue(keys[i], out DataToken value), $"Attempting to get value {keys[i]} from {input}, hit {value.Error}");
                CompareTokens($"Comparing token {keys[i]} in {input}", value, expectedValues[i]);
            }
        }

        private void ValidateJsonToken(string input, DataToken token)
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(input, out DataToken list), $"Attempting to deserialize {input}");
            Assert.AreEqual(list.TokenType, TokenType.DataList);
            Assert.AreEqual(list.DataList.Count, 1);
            Assert.IsTrue(list.DataList.TryGetValue(0, out DataToken value));
            CompareTokens($"comparing token {token} against json {input}",value, token);
        }
        
        [Test]
        public void TestBadJsonObjects()
        {
            ShouldFailToDeserialize(null);
            ShouldFailToDeserialize("a");
            ShouldFailToDeserialize("\"");
            ShouldFailToDeserialize("{");
            ShouldFailToDeserialize("{\"key\"}");
            ShouldFailToDeserialize("{key: 1}");
            ShouldFailToDeserialize("{\"key\":}");
            ShouldFailToDeserialize("{\"key\":\"unfinished string}");
            ShouldFailToDeserialize("{\"key\":\"unfinished object\"");
            ShouldFailToDeserialize("{\"key\":\"bad brackets\"{");
            ShouldFailToDeserialize("{\"key\": +1121}");
        }

        [Test]
        public void TestBadJsonArrays()
        {
            ShouldFailToDeserialize(null);
            ShouldFailToDeserialize("a");
            ShouldFailToDeserialize("\"");
            ShouldFailToDeserialize("[\"");
            ShouldFailToDeserialize("[\"unfinished string]");
            ShouldFailToDeserialize("[\"unfinished array\"");
            ShouldFailToDeserialize("[\"bad brackets\"[");
        }

        [Test]
        public void TestBadJsonTokens()
        {
            ShouldFailToParse("{\"key\": \"\\u00zz\"}");
            ShouldFailToParse("{\"key\": \"\\uD800\"}");
            ShouldFailToParse("{\"key\": 1.0e+}");
            ShouldFailToParse("{\"key\": 1.0e}");
            ShouldFailToParse("{\"key\": 110+21}");
            ShouldFailToParse("{\"key\": 11-21}");
            ShouldFailToParse("{\"key\": 1121e}");
            ShouldFailToParse("{\"key\": --1121}");
            ShouldFailToParse("{\"key\": tru}");
            ShouldFailToParse("{\"key\": fa}");
        }

        [Test]
        public void GenerateGarbage()
        {
            string generated = string.Empty;
            for (int j = 0; j < 100; j++)
            {
                generated = generated.Insert(Random.Range(0, generated.Length), ((char)j).ToString());
                ShouldFailToDeserialize(generated);
            }
        }

        private void ShouldFailToDeserialize(string input)
        {
            Assert.IsFalse(VRCJson.TryDeserializeFromJson(input, out DataToken result), $"Should fail to deserialize {input}");
            Assert.IsTrue(result == DataError.TypeUnsupported || result == DataError.ValueUnsupported || result == DataError.UnableToParse, $"Resulting error should be either Type or Value unsupported, was {result} instead");
        }
        private void ShouldFailToParse(string input)
        {
            Assert.IsTrue(VRCJson.TryDeserializeFromJson(input, out DataToken result), $"Should deserialize {input}");
            Assert.IsFalse(result.DataDictionary.TryGetValue("key", out DataToken value), $"should fail to parse {input}, instead resulted in {value}");
            Assert.AreEqual(DataError.UnableToParse, value, $"Resulting error should be {DataError.UnableToParse}");
        }

        [Test]
        public void TestSerializeToJson()
        {
            //MINIFY
            //lists
            ShouldSerialize(JsonExportType.Minify,new DataList() {true, false, true},  "[true, false, true]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {"a", "b", "c"},  "[\"a\", \"b\", \"c\"]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {new DataToken(), new DataToken(), new DataToken()},  "[null, null, null]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(byte)1, (byte)2, (byte)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(sbyte)1, (sbyte)2, (sbyte)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(short)1, (short)2, (short)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(ushort)1, (ushort)2, (ushort)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(int)1, (int)2, (int)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(uint)1, (uint)2, (uint)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(long)1, (long)2, (long)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(ulong)1, (ulong)2, (ulong)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(float)1, (float)2, (float)3},  "[1, 2, 3]");
            ShouldSerialize(JsonExportType.Minify,new DataList() {(double)1, (double)2, (double)3},  "[1, 2, 3]");
            //Lists inside list
            ShouldSerialize(JsonExportType.Minify,new DataList()
            {
                new DataList(){1, 2, 3}, 
                new DataList() {4, 5, 6}, 
                new DataList() {7, 8, 9}
            },  "[[1, 2, 3], [4, 5, 6], [7, 8, 9]]");
            //Dictionaries inside list
            ShouldSerialize(JsonExportType.Minify,new DataList()
            {
                new DataDictionary() {["a"]=1, ["b"]=2, ["c"]=3}, 
                new DataDictionary() {["d"]=4, ["e"]=5, ["f"]=6}, 
                new DataDictionary() {["g"]=7, ["h"]=8, ["i"]=9}
            },  "[{\"a\": 1, \"b\": 2, \"c\": 3}, {\"d\": 4, \"e\": 5, \"f\": 6}, {\"g\": 7, \"h\": 8, \"i\": 9}]");
            
            //dictionaries
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=true, ["b"]=false, ["c"]=true},  "{\"a\": true, \"b\": false, \"c\": true}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]="a", ["b"]="b", ["c"]="c"},  "{\"a\": \"a\", \"b\": \"b\", \"c\": \"c\"}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=new DataToken(), ["b"]=new DataToken(), ["c"]=new DataToken()},  "{\"a\": null, \"b\": null, \"c\": null}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(byte)1, ["b"]=(byte)2, ["c"]=(byte)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(sbyte)1, ["b"]=(sbyte)2, ["c"]=(sbyte)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(short)1, ["b"]=(short)2, ["c"]=(short)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(ushort)1, ["b"]=(ushort)2, ["c"]=(ushort)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(int)1, ["b"]=(int)2, ["c"]=(int)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(uint)1, ["b"]=(uint)2, ["c"]=(uint)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(long)1, ["b"]=(long)2, ["c"]=(long)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(ulong)1, ["b"]=(ulong)2, ["c"]=(ulong)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(float)1, ["b"]=(float)2, ["c"]=(float)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {["a"]=(double)1, ["b"]=(double)2, ["c"]=(double)3},  "{\"a\": 1, \"b\": 2, \"c\": 3}");
            //Lists inside dictionary
            ShouldSerialize(JsonExportType.Minify,new DataDictionary()
            {
                ["a"]=new DataList() {1, 2, 3}, 
                ["b"]=new DataList() {4, 5, 6}, 
                ["c"]=new DataList() {7, 8, 9}
            },  "{\"a\": [1, 2, 3], \"b\": [4, 5, 6], \"c\": [7, 8, 9]}");
            //Dictionaries inside dictionary
            ShouldSerialize(JsonExportType.Minify,new DataDictionary() {
                ["a"]=new DataDictionary() { ["a"]=1, ["b"]=2, ["c"]=3 }, 
                ["b"]=new DataDictionary() {["d"]=4, ["e"]=5, ["f"]=6}, 
                ["c"]=new DataDictionary() {["g"]=7, ["h"]=8, ["i"]=9}},
                "{\"a\": {\"a\": 1, \"b\": 2, \"c\": 3}, \"b\": {\"d\": 4, \"e\": 5, \"f\": 6}, \"c\": {\"g\": 7, \"h\": 8, \"i\": 9}}");
            
            //BEAUTIFY
            //lists
            ShouldSerialize(JsonExportType.Beautify,new DataList() {true, false, true},  "[\n\ttrue,\n\tfalse,\n\ttrue\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {"a", "b", "c"},  "[\n\t\"a\",\n\t\"b\",\n\t\"c\"\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {new DataToken(), new DataToken(), new DataToken()},  "[\n\tnull,\n\tnull,\n\tnull\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(byte)1, (byte)2, (byte)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(sbyte)1, (sbyte)2, (sbyte)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(short)1, (short)2, (short)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(ushort)1, (ushort)2, (ushort)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(int)1, (int)2, (int)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(uint)1, (uint)2, (uint)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(long)1, (long)2, (long)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(ulong)1, (ulong)2, (ulong)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(float)1, (float)2, (float)3},  "[\n\t1,\n\t2,\n\t3\n]");
            ShouldSerialize(JsonExportType.Beautify,new DataList() {(double)1, (double)2, (double)3},  "[\n\t1,\n\t2,\n\t3\n]");
            //Lists inside list
            ShouldSerialize(JsonExportType.Beautify,new DataList()
            {
                new DataList(){1, 2, 3}, 
                new DataList() {4, 5, 6}, 
                new DataList() {7, 8, 9}
            },  "[\n\t[\n\t\t1,\n\t\t2,\n\t\t3\n\t],\n\t[\n\t\t4,\n\t\t5,\n\t\t6\n\t],\n\t[\n\t\t7,\n\t\t8,\n\t\t9\n\t]\n]");
            //Dictionaries inside list
            ShouldSerialize(JsonExportType.Beautify,new DataList()
            {
                new DataDictionary() {["a"]=1, ["b"]=2, ["c"]=3}, 
                new DataDictionary() {["d"]=4, ["e"]=5, ["f"]=6}, 
                new DataDictionary() {["g"]=7, ["h"]=8, ["i"]=9}
            },  "[\n\t{\n\t\t\"a\": 1,\n\t\t\"b\": 2,\n\t\t\"c\": 3\n\t},\n\t{\n\t\t\"d\": 4,\n\t\t\"e\": 5,\n\t\t\"f\": 6\n\t},\n\t{\n\t\t\"g\": 7,\n\t\t\"h\": 8,\n\t\t\"i\": 9\n\t}\n]");
            
            //Dictionaries
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=true, ["b"]=false, ["c"]=true},  "{\n\t\"a\": true,\n\t\"b\": false,\n\t\"c\": true\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]="a", ["b"]="b", ["c"]="c"},  "{\n\t\"a\": \"a\",\n\t\"b\": \"b\",\n\t\"c\": \"c\"\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=new DataToken(), ["b"]=new DataToken(), ["c"]=new DataToken()},  "{\n\t\"a\": null,\n\t\"b\": null,\n\t\"c\": null\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(byte)1, ["b"]=(byte)2, ["c"]=(byte)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(sbyte)1, ["b"]=(sbyte)2, ["c"]=(sbyte)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(short)1, ["b"]=(short)2, ["c"]=(short)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(ushort)1, ["b"]=(ushort)2, ["c"]=(ushort)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(int)1, ["b"]=(int)2, ["c"]=(int)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(uint)1, ["b"]=(uint)2, ["c"]=(uint)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(long)1, ["b"]=(long)2, ["c"]=(long)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(ulong)1, ["b"]=(ulong)2, ["c"]=(ulong)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(float)1, ["b"]=(float)2, ["c"]=(float)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {["a"]=(double)1, ["b"]=(double)2, ["c"]=(double)3},  "{\n\t\"a\": 1,\n\t\"b\": 2,\n\t\"c\": 3\n}");
            //Lists inside dictionary
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary()
            {
                ["a"]=new DataList() {1, 2, 3}, 
                ["b"]=new DataList() {4, 5, 6}, 
                ["c"]=new DataList() {7, 8, 9}
            },  "{\n\t\"a\": [\n\t\t1,\n\t\t2,\n\t\t3\n\t],\n\t\"b\": [\n\t\t4,\n\t\t5,\n\t\t6\n\t],\n\t\"c\": [\n\t\t7,\n\t\t8,\n\t\t9\n\t]\n}");
            //Dictionaries inside dictionary
            ShouldSerialize(JsonExportType.Beautify,new DataDictionary() {
                ["a"]=new DataDictionary() { ["a"]=1, ["b"]=2, ["c"]=3 }, 
                ["b"]=new DataDictionary() {["d"]=4, ["e"]=5, ["f"]=6}, 
                ["c"]=new DataDictionary() {["g"]=7, ["h"]=8, ["i"]=9}},
                "{\n\t\"a\": {\n\t\t\"a\": 1,\n\t\t\"b\": 2,\n\t\t\"c\": 3\n\t},\n\t\"b\": {\n\t\t\"d\": 4,\n\t\t\"e\": 5,\n\t\t\"f\": 6\n\t},\n\t\"c\": {\n\t\t\"g\": 7,\n\t\t\"h\": 8,\n\t\t\"i\": 9\n\t}\n}");
        }

        private void ShouldSerialize(JsonExportType type, DataToken token,  string expected)
        {
            Assert.IsTrue(VRCJson.TrySerializeToJson(token, type, out DataToken result));
            Assert.AreEqual(expected, result.String);
        }
    }
}
