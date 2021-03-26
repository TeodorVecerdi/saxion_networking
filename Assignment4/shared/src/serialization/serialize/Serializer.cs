using System;
using System.Collections.Generic;
using System.Diagnostics;
using shared.log;
using shared.serialization.model;

namespace shared.serialization {
    public static class Serializer {
        private static readonly Dictionary<SerializationModelKey, SerializationModel> serializationModel =
            new Dictionary<SerializationModelKey, SerializationModel>(SerializationModelKey.EqualityComparer);

        public static byte[] Serialize(object obj, Type type, SerializeMode serializeMode = SerializeMode.Default) => Serialize(obj, type, new Packet(), serializeMode);
        public static byte[] Serialize(object obj, SerializeMode serializeMode = SerializeMode.Default) => Serialize(obj, obj.GetType(), new Packet(), serializeMode);
        public static byte[] Serialize<T>(T obj, SerializeMode serializeMode = SerializeMode.Default) => Serialize(obj, typeof(T), new Packet(), serializeMode);

        internal static byte[] Serialize(object obj, Type type, Packet packet, SerializeMode serializeMode = SerializeMode.Default) {
            if (SerializeUtils.IsTriviallySerializable(type)) {
                if (Options.LOG_SERIALIZATION) Logger.Info($"[TRIVIAL] Serializing type {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
                var bytes = SerializeTrivialImpl(obj, type, packet, serializeMode).GetBytes();
                if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized type {SerializeUtils.FriendlyName(type)} [{bytes.Length} bytes]", null, "SERIALIZE");
                return bytes;
            }

            BeforeSerializeCallback(obj, type);

            if (!HasSerializationModel(type, serializeMode)) BuildSerializationModel(type, serializeMode);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serializing type {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
            var bytes2 = SerializeImpl(obj, type, packet, serializeMode).GetBytes();
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized type {SerializeUtils.FriendlyName(type)} [{bytes2.Length} bytes]", null, "SERIALIZE");
            return bytes2;
        }

        private static Packet SerializeImpl(object obj, Type type, Packet packet, SerializeMode serializeMode) {
            var typeId = type.ID();
            packet.WriteTypeId(typeId);
            packet.Write(serializeMode, SerializeMode.Default);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized SerializeMode[{serializeMode}]", null, "SERIALIZE");

            var model = serializationModel[new SerializationModelKey(typeId, serializeMode)];
            foreach (var field in model.Fields) {
                packet.Write(field.GetValue(obj), serializeMode);
            }

            return packet;
        }

        private static Packet SerializeTrivialImpl(object obj, Type type, Packet packet, SerializeMode serializeMode) {
            var typeId = type.ID();
            packet.WriteTypeId(typeId);
            packet.Write(serializeMode, SerializeMode.Default);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized SerializeMode[{serializeMode}]", null, "SERIALIZE");
            packet.Write(type, obj, serializeMode);
            return packet;
        }

        public static T Deserialize<T>(byte[] data) => (T) Deserialize(new Packet(data));
        public static object Deserialize(byte[] data) => Deserialize(new Packet(data));

        internal static object Deserialize(Packet packet) {
            var typeId = packet.ReadTypeId();
            var serializeMode = packet.Read<SerializeMode>(SerializeMode.Default);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Read SerializeMode[{serializeMode}]", null, "SERIALIZE");
            var type = typeId.Type;
            if (SerializeUtils.IsTriviallySerializable(type)) {
                if (Options.LOG_SERIALIZATION) Logger.Info($"[TRIVIAL] Deserializing type {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
                var objTrivial = DeserializeTrivialImpl(packet, type, serializeMode);
                AfterDeserializeCallback(objTrivial, type);
                return objTrivial;
            }

            if (!HasSerializationModel(type, serializeMode)) BuildSerializationModel(type, serializeMode);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Deserializing type {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
            var obj = DeserializeImpl(packet, type, serializeMode);
            AfterDeserializeCallback(obj, type);
            return obj;
        }

        private static object DeserializeImpl(Packet packet, Type type, SerializeMode serializeMode) {
            var model = serializationModel[new SerializationModelKey(type.ID(), serializeMode)];
            var instance = model.Constructor.Create();
            foreach (var field in model.Fields) {
                var value = packet.Read(field.FieldType, serializeMode);
                field.SetValue(instance, value);
            }

            return instance;
        }

        private static object DeserializeTrivialImpl(Packet packet, Type type, SerializeMode serializeMode) {
            return packet.Read(type, serializeMode);
        }

        private static void BeforeSerializeCallback(object obj, Type type) {
            if (typeof(ISerializationCallback).IsAssignableFrom(type)) {
                var method = type.GetMethod("OnBeforeSerialize");
                Debug.Assert(method != null);
                method.Invoke(obj, new object[0]);
                if (Options.LOG_SERIALIZATION) Logger.Info($"OnBeforeSerialize callback called for {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
                return;
            }

            // Check if UnityEngine serialization callback exists is present
            try {
                var interfaceType = TypeIdUtils.FindTypeByName("UnityEngine.ISerializationCallbackReceiver", true);
                Utils.KeepUnusedVariable(ref interfaceType);
            } catch {
                // do nothing if not present
                return;
            } 
            var unityMethod = type.GetMethod("OnBeforeSerialize");
            Debug.Assert(unityMethod != null);
            unityMethod.Invoke(obj, new object[0]);
            if (Options.LOG_SERIALIZATION) Logger.Info($"OnBeforeSerialize callback called for {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
        }

        private static void AfterDeserializeCallback(object obj, Type type) {
            if (typeof(ISerializationCallback).IsAssignableFrom(type)) {
                var method = type.GetMethod("OnAfterDeserialize");
                Debug.Assert(method != null);
                method.Invoke(obj, new object[0]);
                if (Options.LOG_SERIALIZATION) Logger.Info($"OnAfterDeserialize callback called for {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
                return;
            }

            // Check if UnityEngine serialization callback exists is present
            try {
                var interfaceType = TypeIdUtils.FindTypeByName("UnityEngine.ISerializationCallbackReceiver", true);
                Utils.KeepUnusedVariable(ref interfaceType);
            } catch {
                // do nothing if not present
                return;
            }
            var unityMethod = type.GetMethod("OnAfterDeserialize");
            Debug.Assert(unityMethod != null);
            unityMethod.Invoke(obj, new object[0]);
            if (Options.LOG_SERIALIZATION) Logger.Info($"OnAfterDeserialize callback called for {SerializeUtils.FriendlyName(type)}", null, "SERIALIZE");
        }

        internal static void BuildSerializationModel(Type type, SerializeMode serializeMode) {
            var model = new SerializationModel(type, serializeMode);
            serializationModel[new SerializationModelKey(type.ID(), serializeMode)] = model;
        }

        internal static bool HasSerializationModel(Type type, SerializeMode mode) => serializationModel.ContainsKey(new SerializationModelKey(type.ID(), mode));

        private class SerializationModelKey {
            private readonly TypeId typeId;
            private readonly SerializeMode mode;

            internal SerializationModelKey(TypeId typeId, SerializeMode mode) {
                this.typeId = typeId;
                this.mode = mode;
            }

            internal static Comparer EqualityComparer { get; } = new Comparer();

            internal sealed class Comparer : IEqualityComparer<SerializationModelKey> {
                public bool Equals(SerializationModelKey x, SerializationModelKey y) {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return Equals(x.typeId, y.typeId) && x.mode == y.mode;
                }

                public int GetHashCode(SerializationModelKey obj) {
                    unchecked {
                        return ((obj.typeId != null ? obj.typeId.GetHashCode() : 0) * 397) ^ (int) obj.mode;
                    }
                }
            }
        }
    }
}