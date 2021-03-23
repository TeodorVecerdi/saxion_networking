using System;
using System.Collections.Generic;

namespace shared.serialization {
    public static class TypeIdCache {
        private static readonly Dictionary<Type, int> idCache = new Dictionary<Type, int>();

        public static int Get(Type type) {
            if (idCache.ContainsKey(type)) {
                return idCache[type];
            }

            var typeName = type.AssemblyQualifiedName;
            // is null if it `type` represents a generic type parameter, which should never happen
            if (typeName == null) return -1;
            idCache[type] = typeName.GetDeterministicHashCode();
            return idCache[type];
        }

        // https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        private static int GetDeterministicHashCode(this string str) {
            unchecked {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}