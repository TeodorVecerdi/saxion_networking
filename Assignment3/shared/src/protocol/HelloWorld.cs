namespace shared.protocol {
    public class HelloWorld {
        public float Timeout { get; }
        public int SelfUserId { get; }

        public HelloWorld(float timeout, int selfUserId) {
            Timeout = timeout;
            SelfUserId = selfUserId;
        }
    }
}