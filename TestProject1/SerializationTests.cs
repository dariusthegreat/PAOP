using System;
using System.Reflection;
using Daria.Aop.Weaving;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class SerializationTests
    {
        [TestInitialize]
        public void Initialize()
        {
            _serializer = new BinarySerializer();
        }
        
        [TestMethod]
        public void MethodBaseShouldBeSerializable()
        {
            GiveSomeMethod(WriteLineMethod);
            WhenMethodIsSerialized();
            ThenSerializedDataShouldBeValid();
        }
        
        [TestMethod]
        public void MethodBaseShouldBeDesrializable()
        {
            GiveSomeMethod(WriteLineMethod);
            WhenMethodIsDeserialized();
            ThenDeserializedMethodShouldBeValid();
        }


        private static MethodBase WriteLineMethod
        {
            get
            {
                MethodBase method = typeof(Console).GetMethod(nameof(Console.WriteLine), BindingFlags.Public | BindingFlags.Static, null, new Type[]{typeof(string)}, null);
                Assert.IsNotNull(method, $"Failed to get method info for {nameof(Console.WriteLine)}");
                return method;
            }
        }

        private void GiveSomeMethod(MethodBase method)
        {
            _method = method;
        }

        private void WhenMethodIsSerialized()
        {
            _serialized = _serializer.Serialize(_method);
        }
        
        private void WhenMethodIsDeserialized()
        {
            _serialized = _serializer.Serialize(_method);
            _deserialized = _serializer.Deserialize<MethodInfo>(_serialized);
        }

        private void ThenSerializedDataShouldBeValid()
        {
            Assert.IsNotNull(_serialized, "serialized method is null");
            Assert.IsTrue(_serialized.Length>0, "serialized method bytes should not be empty");
        }
        
        
        private void ThenDeserializedMethodShouldBeValid()
        {
            Assert.IsNotNull(_deserialized, "deserialized object is null");
            Assert.AreEqual(_method, _deserialized, "deserialized not the same as original");
        }


        private BinarySerializer _serializer;
        private MethodBase _method;
        private byte[] _serialized;
        private MethodBase _deserialized;
    }
}