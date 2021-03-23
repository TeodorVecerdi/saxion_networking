namespace shared.protocol {
    public class PositionChanged {
        public int UserId { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public PositionChanged(int userId, float x, float y, float z) {
            UserId = userId;
            X = x;
            Y = y;
            Z = z;
        }
    }
}