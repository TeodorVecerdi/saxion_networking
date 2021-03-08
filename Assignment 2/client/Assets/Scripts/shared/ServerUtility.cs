public static class ServerUtility {
    public static string EncodeMessage(string rawMessage) {
        if (rawMessage.StartsWith("/") || string.Equals(rawMessage, "HEARTBEAT") || rawMessage.StartsWith("TIMEOUT")) return rawMessage;
        return $"MSG:{rawMessage}";
    }

    public static byte[] EncodeMessageAsBytes(string rawMessage) {
        var message = EncodeMessage(rawMessage);
        return System.Text.Encoding.UTF8.GetBytes(message);
    }
}