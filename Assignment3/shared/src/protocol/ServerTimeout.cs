namespace shared.protocol {
    public class ServerTimeout {
        public float Timeout { get; }

        public ServerTimeout(float timeout) {
            Timeout = timeout;
        }
    }
}