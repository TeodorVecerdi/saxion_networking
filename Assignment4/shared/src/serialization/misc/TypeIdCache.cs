using System;
using System.Collections.Generic;

namespace shared.serialization {
    internal class TypeIdCache {
        private readonly Dictionary<int, TypeId> cache = new Dictionary<int, TypeId>();

        internal TypeId GetCached(TypeId typeId) {
            if (!Contains(typeId, out var hash)) {
                
                cache[hash] = typeId;
            }
            return cache[hash];
        }

        internal TypeId GetCached(Type type) {
            if (!Contains(type, out var hash, out var typeId)) cache[hash] = typeId;
            return cache[hash];
        }

        internal TypeId GetCached(string name) {
            if (!Contains(name, out var hash, out var typeId)) cache[hash] = typeId;
            return cache[hash];
        }
        
        internal bool Contains(Type type, out int hashCode, out TypeId typeId) => Contains(typeId = new TypeId(type), out hashCode);
        internal bool Contains(string name, out int hashCode, out TypeId typeId) => Contains(typeId = new TypeId(name), out hashCode);
        internal bool Contains(TypeId typeId, out int hashCode) {
            hashCode = TypeId.Comparer.GetHashCode(typeId);
            return cache.ContainsKey(hashCode);
        }
    }
}