using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace shared {
    public static class SerializationHelper {
        private const string serializeMethodName = "Serialize";
        private const string deserializeMethodName = "Deserialize";
        private static readonly Dictionary<int, SerializerFactory> methods = new Dictionary<int, SerializerFactory>();

        static SerializationHelper() {
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
                methods.Add(serializerType.GUID.GetHashCode(), new SerializerFactory(serialize, deserialize, serializerInst));
            }
        }

        public static void Serialize<T>(T obj, Packet packet) {
            var typeId = typeof(T).GUID.GetHashCode();
            if (!methods.ContainsKey(typeId)) {
                throw new ArgumentException($"Could not find serializer for type {typeof(T)}.", nameof(obj));
            }
            packet.Write(typeId);
            var serializer = methods[typeId];
            serializer.Serialize.Invoke(serializer.Serializer, new object[]{obj, packet});
        }

        public static ISerializable Deserialize(Packet packet) {
            var typeId = packet.ReadInt();
            if (!methods.ContainsKey(typeId)) {
                throw new InvalidOperationException($"Could not find serializer with Type Id {typeId}.");
            }
            var deserializer = methods[typeId];
            var obj = deserializer.Deserialize.Invoke(deserializer.Serializer, new object[] {packet});
            return (ISerializable) obj;
        }
        
        private class SerializerFactory {
            public readonly MethodInfo Serialize;
            public readonly MethodInfo Deserialize;
            public readonly object Serializer;

            public SerializerFactory(MethodInfo serialize, MethodInfo deserialize, object serializer) {
                Serialize = serialize;
                Deserialize = deserialize;
                Serializer = serializer;
            }
        }
    }
}