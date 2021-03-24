namespace shared
{
	/**
	 * Send from SERVER to all CLIENTS to provide info on how many people are in the lobby
	 * and how many of them are ready.
	 */
	public class LobbyInfoUpdate : ASerializable
	{
		public int memberCount;
		public int readyCount;

		public override void Serialize(Packet pPacket)
		{
			pPacket.Write(memberCount);
			pPacket.Write(readyCount);
		}

		public override void Deserialize(Packet pPacket)
		{
			memberCount = pPacket.ReadInt();
			readyCount = pPacket.ReadInt();
		}
	}
}
