using System;
using System.Collections.Generic;

namespace shared.serialization {
    public static class TypeIDCache {
        private static readonly Dictionary<Type, int> idCache = new Dictionary<Type, int>();

        public static int GetID(Type type) {
            if (idCache.ContainsKey(type)) {
                return idCache[type];
            }

            var typeName = type.AssemblyQualifiedName;
            // is null if it `type` represents a generic type parameter, which should never happen
            if(typeName == null) return -1; 
            idCache[type] = typeName.GetHashCode();
            return idCache[type];
        }
    }
}