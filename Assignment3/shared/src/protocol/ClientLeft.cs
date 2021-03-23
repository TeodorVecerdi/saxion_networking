namespace shared.protocol {
    public class ClientLeft {
        public int UserId { get; }

        public ClientLeft(int userId) {
            UserId = userId;
        }
    }
}