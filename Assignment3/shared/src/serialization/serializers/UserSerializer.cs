namespace shared.serialization {
    public class UserSerializer : Serializer<UserModel> {
        public override void Serialize(UserModel obj, Packet packet) {
            packet.Write(obj.UserId);
            packet.Write(obj.SkinId);
            packet.Write(obj.X);
            packet.Write(obj.Y);
            packet.Write(obj.Z);
        }

        public override UserModel Deserialize(Packet packet) {
            return new UserModel(packet.Read<int>(),packet.Read<int>(), packet.Read<float>(), packet.Read<float>(), packet.Read<float>());
        }
    }
}