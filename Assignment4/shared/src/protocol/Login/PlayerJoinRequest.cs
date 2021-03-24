namespace shared
{
    /**
     * Send from CLIENT to SERVER to request joining the server.
     */
    public class PlayerJoinRequest : ASerializable
    {
        public string name;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(name);
        }

        public override void Deserialize(Packet pPacket)
        {
            name = pPacket.ReadString();
        }
    }
}
