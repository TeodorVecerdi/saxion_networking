using System;

namespace shared.serialization {
    [AttributeUsage(AttributeTargets.Constructor)]
    public class PreferredCtorAttribute : Attribute {
        public object[] Arguments { get; }

        /// <summary>Initializes a new instance of the <see cref="T:shared.serialization.attr.PreferredCtorAttribute" /> class.</summary>
        public PreferredCtorAttribute(params object[] arguments) {
            Arguments = arguments;
        }
    }
}