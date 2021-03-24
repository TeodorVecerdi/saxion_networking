using System;
using System.Collections.Generic;

namespace shared.serialization {
    public static class TypeId {
        private const string key = "058CF46F12714A698EAC92DD07C0D6B9:";
        
        private static readonly Dictionary<string, Type> idTypeCache = new Dictionary<string, Type>();

        public static string ID(this Type type) => Get(type);
        public static Type Type(this string typeId) => Get(typeId);

        public static string Get(Type type) {
            return key + type.FullName;
        }

        public static Type Get(string typeId) {
            if (!idTypeCache.ContainsKey(typeId)) idTypeCache[typeId] = System.Type.GetType(typeId.Split(':')[1]);
            return idTypeCache[typeId];
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