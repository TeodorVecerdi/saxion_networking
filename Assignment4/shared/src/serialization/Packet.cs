using System;
using System.Collections;
using System.IO;

namespace shared.serialization {
    /**
	 * The Packet class provides a simple wrapper around an array of bytes (in the form of a MemoryStream), 
	 * that allows us to write/read values to/from the Packet easily. 
	 * Additionally it abstract/decouples how (de)serialization is done from the rest of the application.
	 * 
	 * All the application knows about are packets, no matter how you implemented the (de)serialization itself.
	 */
    public class Packet {
        private readonly BinaryWriter writer; //only used in write mode, to write bytes into a byte array
        private readonly BinaryReader reader; //only used in read mode, to read bytes from a byte array

        /**
		 * Create a Packet for writing.
		 */
        public Packet() {
            //BinaryWriter wraps a Stream, in this case a MemoryStream, which in turn wraps an array of bytes
            writer = new BinaryWriter(new MemoryStream());
        }

        /**
		 * Create a Packet from an existing byte array so we can read from it
		 */
        public Packet(byte[] pSource) {
            //BinaryReader wraps a Stream, in this case a MemoryStream, which in turn wraps an array of bytes
            reader = new BinaryReader(new MemoryStream(pSource));
        }

        public void Write(Type type, object obj) {
            if (Serializer.LOG_SERIALIZATION) Logger.Info($"Writing type {Utils.FriendlyName(type)} [{obj}]", null, "SERIALIZE");
            if (type == typeof(bool)) writer.Write((bool) obj);
            else if (type == typeof(byte)) writer.Write((byte) obj);
            else if (type == typeof(string)) writer.Write((string) obj);
            else if (type == typeof(float)) writer.Write((float) obj);
            else if (type == typeof(double)) writer.Write((double) obj);
            else if (type == typeof(int)) writer.Write((int) obj);
            else if (type == typeof(uint)) writer.Write((uint) obj);
            else if (type == typeof(long)) writer.Write((long) obj);
            else if (Utils.CanSerializeList(type)) WriteList(type, obj);
            else if (type.IsEnum) WriteEnum(type, obj);
            else obj.Serialize(type, this);
        }

        public void Write<T>(T obj) {
            Write(obj.GetType(), obj);
        }

        public object Read(Type type) {
            if (Serializer.LOG_SERIALIZATION) Logger.Info($"Reading type {Utils.FriendlyName(type)}", null, "SERIALIZE");
            if (type == typeof(bool)) return reader.ReadBoolean();
            if (type == typeof(byte)) return reader.ReadByte();
            if (type == typeof(string)) return reader.ReadString();
            if (type == typeof(float)) return reader.ReadSingle();
            if (type == typeof(double)) return reader.ReadDouble();
            if (type == typeof(int)) return reader.ReadInt32();
            if (type == typeof(uint)) return reader.ReadUInt32();
            if (type == typeof(long)) return reader.ReadInt64();
            if (Utils.CanSerializeList(type)) return ReadList(type);
            if (type.IsEnum) return ReadEnum(type);
            return this.Deserialize();
        }

        public T Read<T>() {
            return (T) Read(typeof(T));
        }

        private void WriteList(Type type, object obj) {
            var list = (IList) obj;
            var elemType = Utils.GetListElementType(type);
            Write(list.Count);
            foreach (var elem in list) {
                Write(elemType, elem);
            }
        }

        private object ReadList(Type type) {
            var count = Read<int>();
            var elemType = Utils.GetListElementType(type);
            IList list;
            var isList = false;
            if (Utils.IsList(type)) {
                list = (IList) Activator.CreateInstance(type, count);
                isList = true;
            } else list = Array.CreateInstance(elemType, count);

            for (var i = 0; i < count; i++) {
                if (isList) list.Add(Read(elemType));
                else list[i] = Read(elemType);
            }

            return list;
        }

        private void WriteEnum(Type type, object obj) {
            var underlyingType = type.GetEnumUnderlyingType();
            var underlyingValue = Convert.ChangeType(obj, underlyingType);
            Write(underlyingType, underlyingValue);
        }

        private object ReadEnum(Type type) {
            var underlyingType = type.GetEnumUnderlyingType();
            var underlyingValue = Read(underlyingType);
            return Enum.ToObject(type, underlyingValue);
        }

        /**
		 * Return the bytes that have been written into this Packet.
		 * Only works in Write mode.
		 */
        public byte[] GetBytes() {
            //If we opened the Packet in writing mode, we'll probably need to send it at some point.
            //MemoryStream can either return the whole buffer or simply the part of the buffer that has been filled,
            //which is what we do here using ToArray()
            return ((MemoryStream) writer.BaseStream).ToArray();
        }

        public byte[] GetBytesReadMode() {
            return ((MemoryStream) reader.BaseStream).ToArray();
        }
    }
}