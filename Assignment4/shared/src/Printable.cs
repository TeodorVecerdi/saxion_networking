using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace shared {
    public abstract class Printable {
        public override string ToString() {
            var publicFields = GetType().GetFields().Where(f => f.IsPublic).ToList();
            if (publicFields.Count == 0) return $"\n{GetType().Name}";

            var builder = new StringBuilder();
            builder.Append("\n" + GetType().Name + ":");
            builder.Append("\n----------------------------------------------------------");

            foreach (FieldInfo field in publicFields) {
                var value = field.GetValue(this);
                if (value is ICollection collection) {
                    foreach (var item in collection) builder.Append(item.ToString());
                } else {
                    builder.Append($"\nName: {field.Name} \t\t\t Value: {value}" + "");
                }
            }

            builder.Append("\n----------------------------------------------------------");
            return builder.ToString();
        }
    }
}