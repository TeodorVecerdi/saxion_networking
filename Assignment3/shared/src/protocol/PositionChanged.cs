namespace shared.protocol {
    public class PositionChanged {
        public int UserId { get; }
        public float X { get; }
        public float Z { get; }

        public PositionChanged(int userId, float x, float z) {
            UserId = userId;
            X = x;
            Z = z;
        }
    }
}