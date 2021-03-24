namespace shared
{
	/**
	 * BIDIRECTIONAL Chat message for the lobby
	 */
	public class ChatMessage : ASerializable
	{
		public string message;

		public override void Serialize(Packet pPacket)
		{
			pPacket.Write(message);
		}

		public override void Deserialize(Packet pPacket)
		{
			message = pPacket.ReadString();
		}
	}
}
