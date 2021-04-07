namespace Shared {
    public static class ServerUtility {
        public static string EncodeMessage(string rawMessage, bool skipEncoding = false) {
            if (skipEncoding || rawMessage.StartsWith("/") || string.Equals(rawMessage, "HEARTBEAT") || rawMessage.StartsWith("TIMEOUT")) return rawMessage;
            return $"MSG:{rawMessage}";
        }

        public static byte[] EncodeMessageAsBytes(string rawMessage, bool skipEncoding = false) {
            var message = EncodeMessage(rawMessage, skipEncoding);
            return System.Text.Encoding.UTF8.GetBytes(message);
        }
    }
}