namespace shared
{
    /**
     * Send from CLIENT to SERVER to indicate the move the client would like to make.
     * Since the board is just an array of cells, move is a simple index.
     */
    public class MakeMoveRequest : ASerializable
    {
        public int move;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(move);
        }

        public override void Deserialize(Packet pPacket)
        {
            move = pPacket.ReadInt();
        }
    }
}
