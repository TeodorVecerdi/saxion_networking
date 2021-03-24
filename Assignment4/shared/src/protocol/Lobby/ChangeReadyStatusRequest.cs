namespace shared
{
    /**
     * Send from CLIENT to SERVER to request enabling/disabling the ready status.
     */
    public class ChangeReadyStatusRequest : ASerializable
    {
        public bool ready = false;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(ready);
        }

        public override void Deserialize(Packet pPacket)
        {
            ready = pPacket.ReadBool();
        }
    }
}
