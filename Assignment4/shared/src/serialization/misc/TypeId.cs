using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace shared.serialization {
    internal class TypeId {
        private static int nextId = 0;
        private readonly int currentId;

        private string name;
        private Type type;

        public Type Type => type ?? CalculateType();
        public string ID => name ?? CalculateName();

        public TypeId(Type type) {
            this.type = type;
            currentId = nextId++;
        }

        public TypeId(string name) {
            this.name = name;
            currentId = nextId++;
        }

        private string CalculateName() {
            Debug.Assert(type != null);
            return name = type.FullName;
        }

        private Type CalculateType() {
            Debug.Assert(name != null);
            return type = TypeIdUtils.FindTypeByName(name);
        }

        public static IEqualityComparer<TypeId> Comparer { get; } = new TypeIdComparer();
        
        private sealed class TypeIdComparer : IEqualityComparer<TypeId> {
            public bool Equals(TypeId first, TypeId second) {
                if (first == null || second == null) return first == second;
                if (first.currentId == second.currentId) return true;
                if (first.type != null && second.type != null) return first.type == second.type;
                if (first.name != null && second.name != null) return first.name == second.name;
                return false;
            }

            public int GetHashCode(TypeId obj) {
                if (obj.name == null) obj.CalculateName();
                return obj.name.GetHashCode();
            }
        }
    }
}