using System;
using System.Reflection;

namespace shared.serialization.model {
    public class SerializationConstructor {
        private readonly ConstructorInfo constructor;
        private readonly object[] parameters;

        public SerializationConstructor(ConstructorInfo constructor, object[] parameters) {
            this.constructor = constructor;
            this.parameters = parameters;
        }

        public object Create() {
            return parameters.Length == 0 ? Activator.CreateInstance(constructor.DeclaringType) : constructor.Invoke(parameters);
        }
    }
}