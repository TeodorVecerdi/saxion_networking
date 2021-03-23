using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace shared.serialization {
    public static class SerializationHelper {
        private const string serializeMethodName = "Serialize";
        private const string deserializeMethodName = "Deserialize";
        public static Dictionary<int, SerializerFactory> methods;

        static SerializationHelper() {
            LoadMethods();
        }

        private static void LoadMethods() {
            methods = new Dictionary<int, SerializerFactory>();
            foreach (var serializer in typeof(SerializationHelper).Assembly.GetTypes().Where(t => t.IsClass &&
                                                                                                  !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType &&
                                                                                                  t.BaseType.GetGenericTypeDefinition() == typeof(Serializer<>))) {
                var serializerType = serializer.BaseType.GetGenericArguments()[0];
                MethodInfo serialize = null, deserialize = null;
                foreach (var method in serializer.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                 .Where(method => method.Name == serializeMethodName || method.Name == deserializeMethodName)) {
                    var methodParameters = method.GetParameters();

                    if (method.ReturnType == typeof(void) && methodParameters[0].ParameterType == serializerType && methodParameters[1].ParameterType == typeof(Packet)) {
                        serialize = method;
                    }

                    if (method.ReturnType == serializerType && methodParameters[0].ParameterType == typeof(Packet)) {
                        deserialize = method;
                    }
                }

                if (serialize == null || deserialize == null) {
                    Console.WriteLine($"ERROR: Could not find serialize or deserialize methods for serializer {serializer.Name}");
                    return;
                }

                var serializerInst = Activator.CreateInstance(serializer);
                methods.Add(TypeIdCache.Get(serializerType), new SerializerFactory(serializerType, serialize, deserialize, serializerInst));
            }

            /*foreach (var keyValuePair in methods) {
#if UNITY_EDITOR
                Debug.Log($"Method: {keyValuePair.Value.Serializer.GetType()} => Id: {keyValuePair.Key}");
#else
                Console.WriteLine($"Method: {keyValuePair.Value.Serializer.GetType()} => Id: {keyValuePair.Key} , Name: {keyValuePair.Value.SerializedType.AssemblyQualifiedName}");
#endif
            }*/
        }

        public static byte[] Serialize<T>(T obj, int typeId) {
            if (methods == null || methods.Count == 0) LoadMethods();
            if (!methods.ContainsKey(typeId)) {
                throw new SerializationException($"Could not find Serializer for type {typeof(T)}.");
            }

            var packet = new Packet();
            packet.Write(typeId);
            var serializer = methods[typeId];
            serializer.Serialize.Invoke(serializer.Serializer, new object[] {obj, packet});
            return packet.GetBytes();
        }

        public static byte[] Serialize<T>(T obj) {
            var typeId = TypeIdCache.Get(typeof(T));
            return Serialize(obj, typeId);
        }

        public static object Deserialize(byte[] data) {
            if (methods == null || methods.Count == 0) LoadMethods();
            var packet = new Packet(data);
            var typeId = packet.Read<int>();
            if (!methods.ContainsKey(typeId)) {
                throw new SerializationException($"Could not find serializer with Type Id {typeId}.");
            }

            var deserializer = methods[typeId];
            var obj = deserializer.Deserialize.Invoke(deserializer.Serializer, new object[] {packet});
            return obj;
        }
        
        public static object Deserialize(Packet packet) {
            if (methods == null || methods.Count == 0) LoadMethods();
            var typeId = packet.Read<int>();
            if (!methods.ContainsKey(typeId)) {
                throw new SerializationException($"Could not find serializer with Type Id {typeId}.");
            }

            var deserializer = methods[typeId];
            var obj = deserializer.Deserialize.Invoke(deserializer.Serializer, new object[] {packet});
            return obj;
        }

        public class SerializerFactory {
            public readonly Type SerializedType;
            public readonly MethodInfo Serialize;
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly MethodInfo Deserialize;
            public readonly object Serializer;

            public SerializerFactory(Type serializedType, MethodInfo serialize, MethodInfo deserialize, object serializer) {
                SerializedType = serializedType;
                Serialize = serialize;
                Deserialize = deserialize;
                Serializer = serializer;
            }
        }
    }
}