using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using shared.log;

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
            var size = GetSize(type, obj);
            if (Options.LOG_SERIALIZATION_WRITE) Logger.Info($"Writing type {Utils.FriendlyName(type)} [{(size == -1?"Unknown":size.ToString())} bytes]", null, "SERIALIZE-WRITE");
            if (type == typeof(bool)) writer.Write((bool) obj);
            else if (type == typeof(byte)) writer.Write((byte) obj);
            else if (type == typeof(string)) writer.Write((string) obj);
            else if (type == typeof(float)) writer.Write((float) obj);
            else if (type == typeof(double)) writer.Write((double) obj);
            else if (type == typeof(int)) writer.Write((int) obj);
            else if (type == typeof(uint)) writer.Write((uint) obj);
            else if (type == typeof(long)) writer.Write((long) obj);
            else if (Utils.CanSerializeList(type)) WriteList(type, obj);
            else if (Utils.CanSerializeDictionary(type)) WriteDictionary(type, obj);
            else if (type.IsEnum) WriteEnum(type, obj);
            else obj.Serialize(type, this);
        }

        public void Write<T>(T obj) {
            Write(obj.GetType(), obj);
        }

        public object Read(Type type) {
            if (Options.LOG_SERIALIZATION_READ) Logger.Info($"Reading type {Utils.FriendlyName(type)}", null, "SERIALIZE-READ");
            if (type == typeof(bool)) return reader.ReadBoolean();
            if (type == typeof(byte)) return reader.ReadByte();
            if (type == typeof(string)) return reader.ReadString();
            if (type == typeof(float)) return reader.ReadSingle();
            if (type == typeof(double)) return reader.ReadDouble();
            if (type == typeof(int)) return reader.ReadInt32();
            if (type == typeof(uint)) return reader.ReadUInt32();
            if (type == typeof(long)) return reader.ReadInt64();
            if (Utils.CanSerializeList(type)) return ReadList(type);
            if (Utils.CanSerializeDictionary(type)) return ReadDictionary(type);
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
        
        private void WriteDictionary(Type type, object obj) {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var dict = (IDictionary) obj;
            var keyCollection = dict.Keys;
            var keyList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(keyType));
            Debug.Assert(keyList != null, nameof(keyList) + " != null");
            foreach (var key in keyCollection) {
                keyList.Add(key);
            }
            
            var valueCollection = dict.Values;
            var valueList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType));
            Debug.Assert(valueList != null, nameof(valueList) + " != null");
            foreach (var value in valueCollection) {
                valueList.Add(value);
            }
            WriteList(typeof(List<>).MakeGenericType(keyType), keyList);
            WriteList(typeof(List<>).MakeGenericType(valueType), valueList);
        }

        private object ReadDictionary(Type type) {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            var keyList = (IList) ReadList(typeof(List<>).MakeGenericType(keyType));
            var valueList = (IList) ReadList(typeof(List<>).MakeGenericType(valueType));
            Debug.Assert(keyList != null, nameof(keyList) + " != null");
            Debug.Assert(valueList != null, nameof(valueList) + " != null");
            Debug.Assert(keyList.Count == valueList.Count, $"{nameof(keyList)}.Count == {nameof(valueList)}.Count");
            
            var dict = (IDictionary) Activator.CreateInstance(type);
            Debug.Assert(dict != null, nameof(dict) + " != null");
            for (var i = 0; i < keyList.Count; i++) {
                Debug.Assert(keyList[i] != null, $"{nameof(keyList)}[i] != null");
                dict.Add(keyList[i], valueList[i]);
            }

            return dict;
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

        public void WriteTypeId(TypeId typeId) {
            var id = typeId.ID;
            if (Options.LOG_SERIALIZATION_WRITE) Logger.Info($"Writing TypeId of {Utils.FriendlyName(typeId.Type)} [{id.Length} bytes]", null, "SERIALIZE-WRITE");
            writer.Write(id.Length);
            writer.Write(id);
        }

        public TypeId ReadTypeId() {
            var length = reader.ReadInt32();
            if (Options.LOG_SERIALIZATION_READ) Logger.Info($"Reading TypeId [{length} bytes]", null, "SERIALIZE-READ");
            var id = reader.ReadBytes(length);
            return TypeIdUtil.Get(id);
        }

        private int GetSize(Type type, object obj) {
            if (Utils.BuiltinTypes.Contains(type)) return type.SizeOf();
            if (Utils.IsArray(type)) {
                var elemSize = GetSize(type.GetElementType(), type.GetElementType().Instance());
                if (elemSize == -1) return -1;
                return elemSize * ((Array) obj).GetLength(0);
            }
            if (Utils.IsList(type)) {
                var elemSize = GetSize(type.GetElementType(), type.GetElementType().Instance());
                if (elemSize == -1) return -1;
                return elemSize * ((IList) obj).Count;
            }
            if (Utils.IsDictionary(type)) {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var keySize = GetSize(keyType, keyType.Instance());
                var valueSize = GetSize(valueType, valueType.Instance());
                if (keySize == -1 || valueSize == -1) return -1;
                return (keySize + valueSize) * ((ICollection) obj).Count;
            }
            if (type.IsEnum)return type.GetEnumUnderlyingType().SizeOf();

            return -1;
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