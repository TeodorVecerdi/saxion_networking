using System;
using System.Collections.Generic;

namespace shared.serialization {
    public static class TypeId {
        private static readonly Dictionary<string, Type> idTypeCache = new Dictionary<string, Type>();

        public static string ID(this Type type) => Get(type);
        public static Type AsType(this string typeId) => Get(typeId);

        public static string Get(Type type) {
            return type.FullName;
        }

        public static Type Get(string typeId) {
            if (!idTypeCache.ContainsKey(typeId)) idTypeCache[typeId] = Type.GetType(typeId);
            return idTypeCache[typeId];
        }
    }
}