namespace shared.log {
    public static class Options {
        public static bool LOG_SERIALIZATION = true;
        public static bool LOG_SERIALIZATION_WRITE => true && LOG_SERIALIZATION;
        public static bool LOG_SERIALIZATION_READ => false && LOG_SERIALIZATION;
        public const bool LOG_TYPE_CACHE = true;
    }
}