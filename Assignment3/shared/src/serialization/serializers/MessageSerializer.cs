using shared.protocol;

namespace shared.serialization {
    public class ServerTimeoutSerializer : Serializer<ServerTimeout> {
        public override void Serialize(ServerTimeout obj, Packet packet) {
            packet.Write(obj.Timeout);
        }

        public override ServerTimeout Deserialize(Packet packet) {
            return new ServerTimeout(packet.ReadFloat());
        }
    }
}