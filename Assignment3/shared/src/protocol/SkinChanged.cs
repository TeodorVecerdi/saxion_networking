namespace shared.protocol {
    public class SkinChanged {
        public int UserId { get; }
        public int SkinId { get; }

        public SkinChanged(int userId, int skinId) {
            UserId = userId;
            SkinId = skinId;
        }
    }
}