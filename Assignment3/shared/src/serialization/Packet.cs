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

        /// WRITE METHODS
        private void Write(bool pBool) {
            writer.Write(pBool);
        }

        private void Write(int pInt) {
            writer.Write(pInt);
        }

        private void Write(float @float) {
            writer.Write(@float);
        }

        private void Write(string pString) {
            writer.Write(pString);
        }

        public void Write<T>(T obj) {
            if (typeof(T) == typeof(bool)) {
                Write((bool) (object) obj);
                return;
            }

            if (typeof(T) == typeof(int)) {
                Write((int) (object) obj);
                return;
            }

            if (typeof(T) == typeof(float)) {
                Write((float) (object) obj);
                return;
            }

            if (typeof(T) == typeof(string)) {
                Write((string) (object) obj);
                return;
            }

            writer.Write(SerializationHelper.Serialize(obj));
        }

        /// READ METHODS
        private bool ReadBool() {
            return reader.ReadBoolean();
        }

        private int ReadInt() {
            return reader.ReadInt32();
        }

        private float ReadFloat() {
            return reader.ReadSingle();
        }

        private string ReadString() {
            return reader.ReadString();
        }

        public T Read<T>() {
            if (typeof(T) == typeof(bool)) return (T) (object) ReadBool();
            if (typeof(T) == typeof(int)) return (T) (object) ReadInt();
            if (typeof(T) == typeof(float)) return (T) (object) ReadFloat();
            if (typeof(T) == typeof(string)) return (T) (object) ReadString();
            return (T) SerializationHelper.Deserialize(this);
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