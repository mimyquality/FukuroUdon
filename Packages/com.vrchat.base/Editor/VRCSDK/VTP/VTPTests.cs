using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.Core;

namespace VRC.SDKBase.Editor.VTP
{
    public class VTPTests
    {
        [Test]
        public void TestVTP()
        {
            MemoryStream clientOutServerIn = new MemoryStream();
            MemoryStream clientInServerOut = new MemoryStream();

            MockNetworkStream clientNetworkStream = new MockNetworkStream(clientInServerOut, clientOutServerIn);
            MockNetworkStream serverNetworkStream = new MockNetworkStream(clientOutServerIn, clientInServerOut);

            TestHeartbeat(clientNetworkStream, serverNetworkStream);
            TestHello(clientNetworkStream, serverNetworkStream);

            System.Random random = new System.Random(42);
            int fileSize = random.Next(5 * 1024, 50 * 1024);
            byte[] fileContents = new byte[fileSize];
            random.NextBytes(fileContents);

            TestSendAvatar(fileContents, clientNetworkStream, serverNetworkStream);
            TestSendWorld(fileContents, clientNetworkStream, serverNetworkStream);

            TestError(clientNetworkStream, serverNetworkStream);
        }

        private static void TestHeartbeat(MockNetworkStream clientNetworkStream, MockNetworkStream serverNetworkStream)
        {
            { // CLIENT: Heartbeat from client to server
                using BinaryWriter writer = new BinaryWriter(clientNetworkStream);
                VRChatTestProtocol.WriteHeartbeat(writer);
            }
            { // SERVER: Read heartbeat from client
                using BinaryReader reader = new BinaryReader(serverNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.Heartbeat); // Read packet ID
            }
            { // SERVER: Heartbeat from server to client
                using BinaryWriter writer = new BinaryWriter(serverNetworkStream);
                VRChatTestProtocol.WriteHeartbeat(writer);
            }
            { // CLIENT: Read heartbeat from server
                using BinaryReader reader = new BinaryReader(clientNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.Heartbeat); // Read packet ID
            }
        }
        
        private static void TestHello(MockNetworkStream clientNetworkStream, MockNetworkStream serverNetworkStream)
        {
            int testVersion = 123;
            string testDeviceType = "testDevice";
            string testModel = "testModel";
            { // CLIENT: Hello client to server
                using BinaryWriter writer = new BinaryWriter(clientNetworkStream);
                VRChatTestProtocol.WriteHello(writer, testVersion, testDeviceType, testModel);
            }
            { // SERVER: Read hello from client
                using BinaryReader reader = new BinaryReader(serverNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.HelloMessage); // Read packet ID
                VRChatTestProtocol.ReadHello(reader, out int version, out string deviceType, out string deviceModel);
            
                Assert.AreEqual(testVersion, version);
                Assert.AreEqual(testDeviceType, deviceType);
                Assert.AreEqual(testModel, deviceModel);
            }
            { // SERVER: Send hello to client
                using BinaryWriter writer = new BinaryWriter(serverNetworkStream);
                VRChatTestProtocol.WriteHello(writer, testVersion, testDeviceType, testModel);
            }
            { // CLIENT: Read hello from Server
                using BinaryReader reader = new BinaryReader(clientNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.HelloMessage); // Read packet ID
                VRChatTestProtocol.ReadHello(reader, out int version, out string deviceType, out string deviceModel);
            
                Assert.AreEqual(testVersion, version);
                Assert.AreEqual(testDeviceType, deviceType);
                Assert.AreEqual(testModel, deviceModel);
            }
        }

        public void TestSendAvatar(byte[] fileContents, MockNetworkStream clientNetworkStream, MockNetworkStream serverNetworkStream, int fileSizeLimit = VRChatTestProtocol.MaxFileSize)
        {
            { // SERVER: Send avatar to client
                string path = Path.Combine(Path.GetTempPath(), $"test-avatar-{GUID.Generate()}.bin" );
                File.WriteAllBytes(path, fileContents);
                
                using BinaryWriter writer = new BinaryWriter(serverNetworkStream);
                VRChatTestProtocol.WriteSendAvatar(writer, path, "testAvatar");
                File.Delete(path);
            }
            { // CLIENT: Read avatar from server
                BinaryWriter errorWriter = new BinaryWriter(clientNetworkStream);
                using BinaryReader reader = new BinaryReader(clientNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.SendAvatar); // Read packet ID
                string outputPath = VRChatTestProtocol.ReadAvatarFileToStream(reader, clientNetworkStream, errorWriter, CancellationToken.None, fileSizeLimit);
                byte[] outputBytes = File.ReadAllBytes(outputPath);
                Assert.AreEqual(fileContents, outputBytes);
                File.Delete(outputPath);
            }
        }
        
        public void TestSendWorld(byte[] fileContents, MockNetworkStream clientNetworkStream, MockNetworkStream serverNetworkStream, int fileSizeLimit = VRChatTestProtocol.MaxFileSize)
        {
            { // SERVER: Send world to client
                string path = Path.Combine(Path.GetTempPath(), $"test-world-{GUID.Generate()}.bin" );
                File.WriteAllBytes(path, fileContents);
                
                using BinaryWriter writer = new BinaryWriter(serverNetworkStream);
                VRChatTestProtocol.WriteSendWorld(writer, path);
                File.Delete(path);
            }
            { // CLIENT: Read world from server
                BinaryWriter errorWriter = new BinaryWriter(clientNetworkStream);
                using BinaryReader reader = new BinaryReader(clientNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.SendWorld); // Read packet ID
                string outputPath = VRChatTestProtocol.ReadWorldFileToStream(reader, clientNetworkStream, errorWriter, CancellationToken.None,fileSizeLimit);
                byte[] outputBytes = File.ReadAllBytes(outputPath);
                Assert.AreEqual(fileContents, outputBytes);
                File.Delete(outputPath);
            }
        }
        
        
        [Test]
        public void TestSendAvatarTooBig()
        {
            MemoryStream clientOutServerIn = new MemoryStream();
            MemoryStream clientInServerOut = new MemoryStream();
            
            MockNetworkStream clientNetworkStream = new MockNetworkStream(clientInServerOut, clientOutServerIn);
            MockNetworkStream serverNetworkStream = new MockNetworkStream(clientOutServerIn, clientInServerOut);
            
            Assert.Throws<VTPFileSizeException>(() =>
            {
                TestSendAvatar(new byte[100], clientNetworkStream, serverNetworkStream, fileSizeLimit: 10);
            });
        }
        
        [Test]
        public void TestSendWorldTooBig()
        {
            MemoryStream clientOutServerIn = new MemoryStream();
            MemoryStream clientInServerOut = new MemoryStream();
            
            MockNetworkStream clientNetworkStream = new MockNetworkStream(clientInServerOut, clientOutServerIn);
            MockNetworkStream serverNetworkStream = new MockNetworkStream(clientOutServerIn, clientInServerOut);
            
            Assert.Throws<VTPFileSizeException>(() =>
            {
                TestSendWorld(new byte[100], clientNetworkStream, serverNetworkStream, fileSizeLimit: 10);
            });
        }
        
        
        private void TestError(MockNetworkStream clientNetworkStream, MockNetworkStream serverNetworkStream)
        {
            string errorMessage = "This is a test error.";
            { // CLIENT: Error client to server
                using BinaryWriter writer = new BinaryWriter(clientNetworkStream);
                VRChatTestProtocol.WriteError(writer, errorMessage);
            }
            { // SERVER: Read error from client
                using BinaryReader reader = new BinaryReader(serverNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.Error); // Read packet ID
                string readError = VRChatTestProtocol.ReadError(reader);
                Assert.AreEqual(errorMessage, readError);
            }
            { // SERVER: Send error to client
                using BinaryWriter writer = new BinaryWriter(serverNetworkStream);
                VRChatTestProtocol.WriteError(writer, errorMessage);
            }
            { // CLIENT: Read error from Server
                using BinaryReader reader = new BinaryReader(clientNetworkStream);
                Assert.AreEqual( (VTP_PacketID) reader.ReadInt32(), VTP_PacketID.Error); // Read packet ID
                string readError = VRChatTestProtocol.ReadError(reader);
                Assert.AreEqual(errorMessage, readError);
            }
        }
    }
}