using System;

namespace shared.serialization {
    public static class TypeIdUtil {
        private static readonly TypeIdCache cache = new TypeIdCache();
        public static TypeId ID(this Type type) => Get(type);
        public static TypeId AsType(this string name) => Get(name);

        public static TypeId Get(Type type) => cache.GetCached(type);
        public static TypeId Get(string name) => cache.GetCached(name);
    }
}