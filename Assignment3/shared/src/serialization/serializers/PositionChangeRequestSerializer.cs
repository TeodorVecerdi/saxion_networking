using shared.protocol;

namespace shared.serialization {
    public class PositionChangeRequestSerializer : Serializer<PositionChangeRequest> {
        public override void Serialize(PositionChangeRequest obj, Packet packet) {
            packet.Write(obj.UserId);
            packet.Write(obj.X);
            packet.Write(obj.Y);
            packet.Write(obj.Z);
        }

        public override PositionChangeRequest Deserialize(Packet packet) {
            return new PositionChangeRequest(packet.Read<int>(), packet.Read<float>(), packet.Read<float>(), packet.Read<float>());
        }
    }
}