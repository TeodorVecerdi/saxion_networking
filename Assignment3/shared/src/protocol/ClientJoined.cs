namespace shared.protocol {
    public class ClientJoined {
        public int UserId { get; }
        public int SkinId { get; }
        public float X { get; }
        public float Z { get; }

        public ClientJoined(int userId, int skinId, float x, float z) {
            UserId = userId;
            SkinId = skinId;
            X = x;
            Z = z;
        }
    }
}