namespace shared.log {
    public static class LoggerOptions {
        public static bool LOG_SERIALIZATION { internal get; set; } = true;
        public static bool _LOG_SERIALIZATION_WRITE { private get; set; } = true;
        public static bool _LOG_SERIALIZATION_READ { private get; set; } = true;
        public static bool LOG_SERIALIZATION_WRITE => _LOG_SERIALIZATION_WRITE && LOG_SERIALIZATION;
        public static bool LOG_SERIALIZATION_READ => _LOG_SERIALIZATION_READ && LOG_SERIALIZATION;
    }
}