using System;

namespace shared.serialization {
    public static class TypeIdUtil {
        private static readonly TypeIdCache cache = new TypeIdCache();
        public static TypeId ID(this Type type) => Get(type);
        public static TypeId AsType(this byte[] typeId) => Get(typeId);

        public static TypeId Get(Type type) => cache.GetCached(type);
        public static TypeId Get(byte[] id) => cache.GetCached(id);
    }
}