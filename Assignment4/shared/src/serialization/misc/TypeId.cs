using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace shared.serialization {
    public class TypeId {
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
            return type = FindType(name);
        }

        public static IEqualityComparer<TypeId> Comparer { get; } = new TypeIdComparer();

        private static Type FindType(string name) {
            var type = Type.GetType(name);
            if (type != null)
                return type;

            try {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                //To speed things up, we check first in the already loaded assemblies.
                foreach (var assembly in assemblies) {
                    type = assembly.GetType(name);
                    if (type != null)
                        return type;
                }

                var loadedAssemblies = assemblies.ToList();
                foreach (var loadedAssembly in assemblies) {
                    foreach (var referencedAssemblyName in loadedAssembly.GetReferencedAssemblies()) {
                        if (loadedAssemblies.Any(x => x.GetName() == referencedAssemblyName)) continue;
                        
                        try {
                            var referencedAssembly = Assembly.Load(referencedAssemblyName);
                            type = referencedAssembly.GetType(name);
                            if (type != null)
                                return type;
                            
                            loadedAssemblies.Add(referencedAssembly);
                        } catch {
                            //We will ignore this, because the Type might still be in one of the other Assemblies.
                        }
                    }
                }
            } catch (Exception e) {
                Logger.Except(e, new TypeId((string)null), true, true, true);
                throw;
            }

            if (type != null) return type;
            
            var exception = new Exception($"Could not find type {name} in any loaded or referenced assembly.");
            Logger.Except(exception, new TypeId((string) null), true, true, true);
            throw exception;
        }

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