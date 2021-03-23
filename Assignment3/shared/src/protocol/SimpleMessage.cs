namespace shared {
    public class SimpleMessage {
        private string text;

        public void Serialize(Packet pPacket) {
            pPacket.Write(text);
        }

        public void Deserialize(Packet pPacket) {
            text = pPacket.ReadString();
        }
    }
}