using System;
using System.IO;

namespace shared
{
	/**
	 * The Packet class provides a simple wrapper around an array of bytes (in the form of a MemoryStream), 
	 * that allows us to write/read values to/from the Packet easily. 
	 */
	public class Packet
	{
		private BinaryWriter writer;	//only used in write mode, to write bytes into a byte array
		private BinaryReader reader;	//only used in read mode, to read bytes from a byte array

		/**
		 * Create a Packet for writing.
		 */
		public Packet()
		{
			//BinaryWriter wraps a Stream, in this case a MemoryStream, which in turn wraps an array of bytes
			writer = new BinaryWriter(new MemoryStream());
		}

		/**
		 * Create a Packet from an existing byte array so we can read from it
		 */
		public Packet (byte[] pSource)
		{
			//BinaryReader wraps a Stream, in this case a MemoryStream, which in turn wraps an array of bytes
			reader = new BinaryReader(new MemoryStream(pSource));
		}

		/// WRITE METHODS

		public void Write (int pInt)							{		writer.Write(pInt);			}
		public void Write (string pString)						{		writer.Write(pString);		}
		public void Write (bool pBool)							{		writer.Write(pBool);		}
		
		public void Write (ASerializable pSerializable)			{
			//write the full classname into the stream first
			Write(pSerializable.GetType().FullName);
			//then ask the serializable object to serialize itself
			pSerializable.Serialize(this); 
		}

		/// READ METHODS

		public int ReadInt() { return reader.ReadInt32(); }
		public string ReadString() { return reader.ReadString(); }
		public bool ReadBool() { return reader.ReadBoolean(); }

		public ASerializable ReadObject() 
		{
			//get the classname from the stream first
			Type type = Type.GetType(ReadString());
			//create an instance of it through reflection (requires default constructor)
			ASerializable obj = (ASerializable)Activator.CreateInstance(type);
			obj.Deserialize(this);
			return obj;
		}

		/**
		 * Convenience method to read AND cast an object in one go.
		 */
		public T Read<T>() where T:ASerializable
		{
			return (T)ReadObject();
		}

		/**
		 * Return the bytes that have been written into this Packet.
		 * Only works in Write mode.
		 */
		public byte[] GetBytes()
		{
			//If we opened the Packet in writing mode, we'll probably need to send it at some point.
			//MemoryStream can either return the whole buffer or simply the part of the buffer that has been filled,
			//which is what we do here using ToArray()
			return ((MemoryStream)writer.BaseStream).ToArray();
		}

		/**
		 * Helper method to find out if the Packet has more data to read.
		 */
		public bool HasMoreData()
		{
			if (reader == null) return false;

			MemoryStream memoryStream = (MemoryStream)reader.BaseStream;
			return memoryStream.Position < memoryStream.Length;
		}

	}
}
