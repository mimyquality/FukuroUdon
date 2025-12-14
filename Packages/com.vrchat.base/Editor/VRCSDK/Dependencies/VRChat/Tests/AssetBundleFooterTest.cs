using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VRCCore;

namespace VRC.SDKBase.Editor.Tests
{
    public class AssetBundleFooterTest
    {
        // This is the asset bundle header, but with no actual data
        // (total file size reported by header == header size)
        private readonly byte[] dummyEmptyAssetBundleBytes = new byte[]
        {
            0x55,0x6E,0x69,0x74, 0x79, 0x46, 0x53, 0x00,                         // 'UnityFS' null terminated
            0x00,0x00,0x00,0x08,                                                 // uint32 File version 8
            0x35,0x2E,0x78,0x2E, 0x78, 0x00,                                     // '5.x.x' null terminated
            0x32,0x30,0x32,0x32, 0x2E, 0x33, 0x2E, 0x32, 0x32, 0x66, 0x31, 0x00, // '2022.3.22f' null terminated
            0x00,0x00,0x00,0x00, 0x00, 0x00, 0x00, 0x32,                         // total file size = 0x32 = 50 bytes
            0x00,0x00,0x00,0x00,                                                 // Compressed size
            0x00,0x00,0x00,0x00,                                                 // Decomrpesseed size
            0x00,0x00,0x00,0x00,                                                 // Flags
        };
        
        [Test]
        public void TestAssetBundleFooter()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(dummyEmptyAssetBundleBytes, 0, dummyEmptyAssetBundleBytes.Length);

            byte[] testBytes = Encoding.ASCII.GetBytes("this is a bunch of test bytes");
            AssetBundleFooter.AppendToStream(stream, "test", testBytes);
            
            byte[] thumbnailBytes = Encoding.ASCII.GetBytes("Pretend this is a thumbnail");
            AssetBundleFooter.AppendToStream(stream, "thumbnail", thumbnailBytes);
            
            
            stream.Seek(0, SeekOrigin.Begin);
            List<FooterSection> footerSections = AssetBundleFooter.GetFooterSections(stream);
            Assert.AreEqual(2, footerSections.Count);
            Assert.AreEqual("test", footerSections[0].sectionType);
            Assert.AreEqual("thumbnail", footerSections[1].sectionType);
            
            Assert.IsTrue(testBytes.SequenceEqual(footerSections[0].data));
            Assert.IsTrue(thumbnailBytes.SequenceEqual(footerSections[1].data));
        }
        
        [Test]
        public void TestEmptyAssetBundleFooter()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(dummyEmptyAssetBundleBytes, 0, dummyEmptyAssetBundleBytes.Length);
            
            stream.Seek(0, SeekOrigin.Begin);
            List<FooterSection> footerSections = AssetBundleFooter.GetFooterSections(stream);
            Assert.AreEqual(0, footerSections.Count);
        }
        
        [Test]
        public void TestAssetBundleFooterWithGarbageAtEnd()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(dummyEmptyAssetBundleBytes, 0, dummyEmptyAssetBundleBytes.Length);
            stream.Write(Encoding.ASCII.GetBytes("Who knows why there's garbage at the end, but it's not a footer!"));

            List<FooterSection> footerSections = null;
            Assert.DoesNotThrow(() =>
            {
                stream.Seek(0, SeekOrigin.Begin);
                footerSections = AssetBundleFooter.GetFooterSections(stream);
            });
            
            Assert.AreEqual(0, footerSections.Count);
        }

        [Test]
        public void TestNonUnityBundle()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(Encoding.ASCII.GetBytes("some non-unity bundle bytes"));
            Assert.DoesNotThrow(() =>
            {
                byte[] testBytes = Encoding.ASCII.GetBytes("this is a bunch of test bytes");
                AssetBundleFooter.AppendToStream(stream, "test", testBytes);
            });
            
            Exception exception = Assert.Throws<Exception>(() =>
            {
                stream.Seek(0, SeekOrigin.Begin);
                List<FooterSection> footerSections = AssetBundleFooter.GetFooterSections(stream);
            });
            Assert.IsTrue(exception.Message.ToLower().Contains("not a valid unity asset bundle"));
        }
    }
}