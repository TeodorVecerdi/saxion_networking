namespace shared {
    public class ServerTimeout : ISerializable {
        private readonly float timeout;
        public float Timeout => timeout;

        public ServerTimeout(float timeout) {
            this.timeout = timeout;
        }
    }
}