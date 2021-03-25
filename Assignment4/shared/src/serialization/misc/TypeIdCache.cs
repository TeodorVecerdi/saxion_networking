using System;
using System.Collections.Generic;

namespace shared.serialization {
    public class TypeIdCache {
        private readonly Dictionary<int, TypeId> cache = new Dictionary<int, TypeId>();

        public TypeId GetCached(TypeId typeId) {
            if (!Contains(typeId, out var hash)) {
                
                cache[hash] = typeId;
            }
            return cache[hash];
        }

        public TypeId GetCached(Type type) {
            if (!Contains(type, out var hash, out var typeId)) cache[hash] = typeId;
            return cache[hash];
        }

        public TypeId GetCached(byte[] id) {
            if (!Contains(id, out var hash, out var typeId)) cache[hash] = typeId;
            return cache[hash];
        }
        
        public bool Contains(Type type, out int hashCode, out TypeId typeId) => Contains(typeId = new TypeId(type), out hashCode);
        public bool Contains(byte[] id, out int hashCode, out TypeId typeId) => Contains(typeId = new TypeId(id), out hashCode);
        public bool Contains(TypeId typeId, out int hashCode) {
            hashCode = TypeId.Comparer.GetHashCode(typeId);
            return cache.ContainsKey(hashCode);
        }
    }
}