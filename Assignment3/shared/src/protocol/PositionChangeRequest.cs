namespace shared.protocol {
    public class PositionChangeRequest {
        public int UserId { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public PositionChangeRequest(int userId, float x, float y, float z) {
            UserId = userId;
            X = x;
            Y = y;
            Z = z;
        }
    }
}