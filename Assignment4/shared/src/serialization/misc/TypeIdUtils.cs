﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace shared.serialization {
    public static class TypeIdUtils {
        private static readonly TypeIdCache cache = new TypeIdCache();
        public static TypeId ID(this Type type) => Get(type);
        public static TypeId AsType(this string name) => Get(name);

        public static TypeId Get(Type type) => cache.GetCached(type);
        public static TypeId Get(string name) => cache.GetCached(name);

        private static ConcurrentDictionary<string, Type> typeCache;
        public static Type FindTypeByName(string name) {
            if(typeCache == null) typeCache = new ConcurrentDictionary<string, Type>();
            if (typeCache == null) {
                var e = new Exception("Could not create type cache.");
                Logger.Except(e, new TypeId((string) null), true, true, true);
                throw e;
            }

            if (typeCache.ContainsKey(name)) return typeCache[name];
            
            var type = Type.GetType(name);
            if (type != null) {
                typeCache[name] = type;
                return type;
            }

            try {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                //To speed things up, we check first in the already loaded assemblies.
                foreach (var assembly in assemblies) {
                    type = assembly.GetType(name);
                    if (type == null) continue;
                    
                    typeCache[name] = type;
                    return type;
                }

                var loadedAssemblies = assemblies.ToList();
                foreach (var loadedAssembly in assemblies) {
                    foreach (var referencedAssemblyName in loadedAssembly.GetReferencedAssemblies()) {
                        if (loadedAssemblies.Any(x => x.GetName() == referencedAssemblyName)) continue;
                        
                        try {
                            var referencedAssembly = Assembly.Load(referencedAssemblyName);
                            type = referencedAssembly.GetType(name);
                            if (type != null) {
                                typeCache[name] = type;
                                return type;
                            }
                            
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

            if (type != null) {
                typeCache[name] = type;
                return type;
            }
            
            var exception = new Exception($"Could not find type {name} in any loaded or referenced assembly.");
            Logger.Except(exception, new TypeId((string) null), true, true, true);
            throw exception;
        }
    }
}