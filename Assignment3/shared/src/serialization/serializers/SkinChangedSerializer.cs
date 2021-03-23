using shared.protocol;

namespace shared.serialization {
    public class SkinChangedSerializer : Serializer<SkinChanged> {
        public override void Serialize(SkinChanged obj, Packet packet) {
            packet.Write(obj.UserId);
            packet.Write(obj.SkinId);
        }

        public override SkinChanged Deserialize(Packet packet) {
            return new SkinChanged(packet.Read<int>(), packet.Read<int>());
        }
    }
}