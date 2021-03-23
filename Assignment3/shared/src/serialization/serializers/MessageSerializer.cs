using shared.protocol;

namespace shared.serialization {
    public class MessageSerializer : Serializer<Message> {
        public override void Serialize(Message obj, Packet packet) {
            packet.Write(obj.Text);
        }

        public override Message Deserialize(Packet packet) {
            return new Message(packet.Read<string>());
        }
    }
}