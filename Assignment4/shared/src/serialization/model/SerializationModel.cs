using System;
using System.Linq;
using System.Reflection;

namespace shared.serialization.model {
    internal class SerializationModel {
        public readonly SerializationConstructor Constructor;
        public readonly FieldInfo[] Fields;

        internal SerializationModel(Type type) {
            var ctor = type.Ctor();
            var parameters = Utils.CtorParameters(ctor);
            Constructor = new SerializationConstructor(ctor.Constructor, parameters);

            Fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(Utils.CanSerializeField).ToArray();
            foreach (var field in Fields) {
                if(!Utils.BuiltinTypes.Contains(field.FieldType) && !Serializer.HasSerializationModel(field.FieldType))
                    Serializer.BuildSerializationModel(field.FieldType);
            }
        }
    }
}