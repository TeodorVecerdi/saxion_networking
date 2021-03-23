using shared.protocol;

namespace shared.serialization {
    public class ClientJoinedSerializer : Serializer<ClientJoined> {
        public override void Serialize(ClientJoined obj, Packet packet) {
            packet.Write(obj.UserId);
            packet.Write(obj.SkinId);
            packet.Write(obj.X);
            packet.Write(obj.Y);
            packet.Write(obj.Z);
        }

        public override ClientJoined Deserialize(Packet packet) {
            return new ClientJoined(packet.Read<int>(),packet.Read<int>(), packet.Read<float>(), packet.Read<float>(), packet.Read<float>());
        }
    }
}