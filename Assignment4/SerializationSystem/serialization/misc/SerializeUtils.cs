﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SerializationSystem.Logging;

namespace SerializationSystem.Internal {
    internal static class SerializeUtils {
        
        internal static bool HasSerializedAttr(FieldInfo field) => field.GetCustomAttribute<SerializedAttribute>() != null;
        internal static bool HasNonSerializedAttr(FieldInfo field) => field.GetCustomAttribute<NonSerializedAttribute>() != null;
        internal static bool IsTriviallySerializable(Type type) => BuiltinTypes.Contains(type) || type.IsEnum || CanSerializeList(type) || CanSerializeDictionary(type);

        internal static InstantiateCtor Ctor(this Type type) {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructors.Length == 0) return null;
            //Find suitable constructor
            ConstructorInfo defaultCtor = null;
            foreach (var ctor in constructors) {
                // Preferred ctor
                var attr = ctor.GetCustomAttribute<PreferredCtorAttribute>();
                if (attr != null) return new InstantiateCtor(ctor, attr);

                // Default ctor
                if (ctor.GetParameters().Length == 0) defaultCtor = ctor;
            }

            var finalCtor = defaultCtor ?? constructors[0];
            return new InstantiateCtor(finalCtor, null);
        }

        internal static object Instantiate(Type type, SerializationConstructor ctor) {
            return BuiltinTypes.Contains(type) ? InstantiateTrivial(type) : InstantiateNonTrivial(type, ctor);
        }

        internal static object Instantiate(Type type) {
            if (BuiltinTypes.Contains(type))
                return InstantiateTrivial(type);
            var ctor = type.Ctor();
            var parameters = CtorParameters(ctor);
            return InstantiateNonTrivial(type, new SerializationConstructor(ctor.Constructor, parameters));
        }

        internal static object Instance(this Type type) => Instantiate(type);
        internal static object Instance(this Type type, SerializationConstructor ctor) => Instantiate(type, ctor);

        internal static T Instantiate<T>() {
            return (T) Instantiate(typeof(T));
        }

        internal static T Instantiate<T>(SerializationConstructor ctor) {
            return (T) Instantiate(typeof(T), ctor);
        }

        private static object InstantiateTrivial(Type type) {
            return type.Default();
        }

        private static object InstantiateNonTrivial(Type type, SerializationConstructor ctor) {
            return ctor.Create();
        }

        internal static object[] CtorParameters(InstantiateCtor ctor) {
            if (ctor == null) return new object[0];
            var parameters = ctor.Constructor.GetParameters();
            if (parameters.Length == 0) return new object[0];

            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) {
                if (ctor.Attribute != null && i < ctor.Attribute.Arguments.Length && CanCast(ctor.Attribute.Arguments[i].GetType(), parameters[i].ParameterType)) {
                    var val = ctor.Attribute.Arguments[i];
                    if (ctor.Attribute.Arguments[i].GetType() != parameters[i].ParameterType)
                        val = Convert.ChangeType(val, parameters[i].ParameterType);
                    args[i] = val;
                    continue;
                }

                if (parameters[i].HasDefaultValue) args[i] = parameters[i].DefaultValue;
                else args[i] = parameters[i].ParameterType.Default();
            }

            return args;
        }

        internal static object Default(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        internal static bool CanCast(Type from, Type to) {
            if (from == null || to == null) return false;
            if (to.IsAssignableFrom(from)) return true;
            try {
                Convert.ChangeType(Activator.CreateInstance(from), to);
                return true;
            } catch {
                return false;
            }
        }

        internal static string FriendlyName(Type type) {
            try {
                if (BuiltinTypes.Contains(type)) return BuiltinFriendlyName(type);
                if (type.IsGenericType) return GenericFriendlyName(type);
                if (type.IsArray) return ArrayFriendlyName(type);
                return type.Name;
            } catch {
                Log.Error($"Could not get friendly name for type {type.FullName}");
                return type.FullName;
            }
        }

        internal static string GenericFriendlyName(Type type) {
            var baseTypeName = type.Name.Split('`')[0];
            var argumentTypes = type.GetGenericArguments().Select(FriendlyName);
            return $"{baseTypeName}<{string.Join(",", argumentTypes)}>";
        }

        internal static string ArrayFriendlyName(Type type) {
            var baseType = type.GetElementType();
            var dimension = new string(',', type.GetArrayRank() - 1);
            return $"{FriendlyName(baseType)}[{dimension}]";
        }

        internal static string BuiltinFriendlyName(Type type) {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(string)) return "string";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            return type.Name;
        }

        internal static bool IsArray(Type type) => type.IsArray && type.GetArrayRank() == 1;
        internal static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        internal static bool IsDictionary(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        internal static bool CanSerializeList(Type type) => IsArray(type) || IsList(type);
        internal static bool CanSerializeDictionary(Type type) => IsDictionary(type);
        internal static Type GetListElementType(Type type) => IsList(type) ? type.GenericTypeArguments[0] : type.GetElementType();
        internal static Type GetDictionaryKeyType(Type type) => type.GenericTypeArguments[0];
        internal static Type GetDictionaryValueType(Type type) => type.GenericTypeArguments[1];

        internal static readonly HashSet<Type> BuiltinTypes = new HashSet<Type> {
            typeof(bool), typeof(byte), typeof(sbyte), typeof(char), 
            typeof(decimal), typeof(double), typeof(float), typeof(int), 
            typeof(uint), typeof(long), typeof(ulong), typeof(short), 
            typeof(ushort), typeof(string)
        };

        internal class InstantiateCtor {
            public readonly ConstructorInfo Constructor;
            public readonly PreferredCtorAttribute Attribute;

            public InstantiateCtor(ConstructorInfo constructor, PreferredCtorAttribute attribute) {
                Constructor = constructor;
                Attribute = attribute;
            }
        }
    }
}