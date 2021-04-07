namespace shared.protocol {
    public class Message {
        public int UserId { get; }
        public string Text { get; }

        public Message(int userId, string text) {
            UserId = userId;
            Text = text;
        }
    }
}