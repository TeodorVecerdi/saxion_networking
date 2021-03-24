using System;
using System.Collections.Generic;
using shared.serialization.model;

namespace shared.serialization {
    public static class Serializer {
        public const bool LOG_SERIALIZATION = true;
        private static readonly Dictionary<string, SerializationModel> serializationModel = new Dictionary<string, SerializationModel>();

        public static byte[] Serialize(object obj, Type type) => Serialize(obj, type, new Packet());
        public static byte[] Serialize<T>(T obj, Packet packet) => Serialize(obj, typeof(T), new Packet());
        public static byte[] Serialize<T>(this T obj) => Serialize(obj, typeof(T), new Packet());

        public static byte[] Serialize(this object obj, Type type, Packet packet) {
            if (LOG_SERIALIZATION) Logger.Info($"Serializing type {Utils.FriendlyName(type)}", "SERIALIZE");
            if (Utils.IsTriviallySerializable(type)) {
                return SerializeTrivialImpl(obj, type, packet).GetBytes();
            }

            if (!HasSerializationModel(type)) BuildSerializationModel(type);
            return SerializeImpl(obj, type, packet).GetBytes();
        }

        private static Packet SerializeImpl(object obj, Type type, Packet packet) {
            var typeId = type.ID();
            packet.Write(typeId);

            var model = serializationModel[typeId];
            foreach (var field in model.Fields) {
                packet.Write(field.GetValue(obj));
            }

            return packet;
        }

        private static Packet SerializeTrivialImpl(object obj, Type type, Packet packet) {
            if (LOG_SERIALIZATION) Logger.Info($"^----TRIVIAL", "SERIALIZE");
            var typeId = type.ID();
            packet.Write(typeId);
            packet.Write(type, obj);
            return packet;
        }

        public static T Deserialize<T>(this byte[] data) => (T) Deserialize(new Packet(data));
        public static object Deserialize(this byte[] data) => Deserialize(new Packet(data));
        public static T Deserialize<T>(this Packet packet) => (T) Deserialize(packet);

        public static object Deserialize(this Packet packet) {
            var type = packet.Read<string>().AsType();
            if (LOG_SERIALIZATION) Logger.Info($"Deserializing type {Utils.FriendlyName(type)}", "SERIALIZE");
            if (Utils.IsTriviallySerializable(type)) return DeserializeTrivialImpl(packet, type);

            if (!HasSerializationModel(type)) BuildSerializationModel(type);
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
            if (LOG_SERIALIZATION) Logger.Info($"^----TRIVIAL", "SERIALIZE");
            return packet.Read(type);
        }

        internal static void BuildSerializationModel(Type type) {
            var model = new SerializationModel(type);
            serializationModel[type.ID()] = model;
        }

        internal static bool HasSerializationModel(Type type) => serializationModel.ContainsKey(type.ID());
    }
}