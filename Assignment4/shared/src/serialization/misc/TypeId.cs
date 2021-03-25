using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace shared.serialization {
    public class TypeId {
        private static int nextId = 0;
        private readonly int currentId;
        
        private byte[] id;
        private Type type;

        public Type Type => type ?? CalculateType();
        public byte[] ID => id ?? CalculateID();
        
        public TypeId(Type type) {
            this.type = type;
            currentId = nextId++;
        }

        public TypeId(byte[] id) {
            this.id = id;
            currentId = nextId++;
        }

        private byte[] CalculateID() {
            Debug.Assert(type != null);
            return id = System.Text.Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);
        }

        private Type CalculateType() {
            Debug.Assert(id != null);
            return type = Type.GetType( System.Text.Encoding.UTF8.GetString(id));
        }

        public static IEqualityComparer<TypeId> Comparer { get; } = new TypeIdComparer();

        private sealed class TypeIdComparer : IEqualityComparer<TypeId> {
            public bool Equals(TypeId first, TypeId second) {
                if (first == null || second == null) return first == second;
                if (first.currentId == second.currentId) return true;
                if (first.type != null && second.type != null) return first.type == second.type;
                if (first.id != null && second.id != null) return first.id.SequenceEqual(second.id);
                return false;
            }

            public int GetHashCode(TypeId obj) {
                if (obj.type == null) obj.CalculateType();
                return obj.type.GetHashCode();
            }
        }
    }
}