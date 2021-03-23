namespace shared.protocols {
    public class ServerTimeout {
        private readonly float timeout;
        public float Timeout => timeout;

        public ServerTimeout(float timeout) {
            this.timeout = timeout;
        }
    }
}