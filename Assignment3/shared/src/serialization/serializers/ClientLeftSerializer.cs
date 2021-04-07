using shared.protocol;

namespace shared.serialization {
    public class ClientLeftSerializer : Serializer<ClientLeft> {
        public override void Serialize(ClientLeft obj, Packet packet) {
            packet.Write(obj.UserId);
        }

        public override ClientLeft Deserialize(Packet packet) {
            return new ClientLeft(packet.Read<int>());
        }
    }
}