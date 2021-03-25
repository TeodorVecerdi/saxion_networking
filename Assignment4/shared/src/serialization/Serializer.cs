using System;
using System.Collections.Generic;
using shared.log;
using shared.serialization.model;

namespace shared.serialization {
    public static class Serializer {
        private static readonly Dictionary<TypeId, SerializationModel> serializationModel = new Dictionary<TypeId, SerializationModel>();

        public static byte[] Serialize(this object obj, Type type) => Serialize(obj, type, new Packet());
        public static byte[] Serialize(this object obj) => Serialize(obj, obj.GetType(), new Packet());
        public static byte[] Serialize<T>(this T obj, Packet packet) => Serialize(obj, typeof(T), new Packet());
        public static byte[] Serialize<T>(this T obj) => Serialize(obj, typeof(T), new Packet());

        public static byte[] Serialize(this object obj, Type type, Packet packet) {
            if (Utils.IsTriviallySerializable(type)) {
                if (Options.LOG_SERIALIZATION) Logger.Info($"[TRIVIAL] Serializing type {Utils.FriendlyName(type)}", null, "SERIALIZE");
                var bytes = SerializeTrivialImpl(obj, type, packet).GetBytes();
                if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized type {Utils.FriendlyName(type)} [{bytes.Length} bytes]", null, "SERIALIZE");
                return bytes;
            }

            if (!HasSerializationModel(type)) BuildSerializationModel(type);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serializing type {Utils.FriendlyName(type)}", null, "SERIALIZE");
            var bytes2 = SerializeImpl(obj, type, packet).GetBytes();
            if (Options.LOG_SERIALIZATION) Logger.Info($"Serialized type {Utils.FriendlyName(type)} [{bytes2.Length} bytes]", null, "SERIALIZE");
            return bytes2;
        }

        private static Packet SerializeImpl(object obj, Type type, Packet packet) {
            var typeId = type.ID();
            packet.WriteTypeId(typeId);

            var model = serializationModel[typeId];
            foreach (var field in model.Fields) {
                packet.Write(field.GetValue(obj));
            }

            return packet;
        }

        private static Packet SerializeTrivialImpl(object obj, Type type, Packet packet) {
            var typeId = type.ID();
            packet.WriteTypeId(typeId);
            packet.Write(type, obj);
            return packet;
        }

        public static T Deserialize<T>(this byte[] data) => (T) Deserialize(new Packet(data));
        public static object Deserialize(this byte[] data) => Deserialize(new Packet(data));
        public static T Deserialize<T>(this Packet packet) => (T) Deserialize(packet);

        public static object Deserialize(this Packet packet) {
            var typeId = packet.ReadTypeId();
            var type = typeId.Type;
            if (Utils.IsTriviallySerializable(type)) {
                if (Options.LOG_SERIALIZATION) Logger.Info($"[TRIVIAL] Deserializing type {Utils.FriendlyName(type)}", null, "SERIALIZE");
                return DeserializeTrivialImpl(packet, type);
            }

            if (!HasSerializationModel(type)) BuildSerializationModel(type);
            if (Options.LOG_SERIALIZATION) Logger.Info($"Deserializing type {Utils.FriendlyName(type)}", null, "SERIALIZE");
            return DeserializeImpl(packet, type);
        }

        private static object DeserializeImpl(Packet packet, Type type) {
            var model = serializationModel[type.ID()];
            var instance = model.Constructor.Create();
            foreach (var field in model.Fields) {
                var value = packet.Read(field.FieldType);
                field.SetValue(instance, value);
            }

            return instance;
        }

        private static object DeserializeTrivialImpl(Packet packet, Type type) {
            return packet.Read(type);
        }

        internal static void BuildSerializationModel(Type type) {
            var model = new SerializationModel(type);
            serializationModel[type.ID()] = model;
        }

        internal static bool HasSerializationModel(Type type) => serializationModel.ContainsKey(type.ID());
    }
}