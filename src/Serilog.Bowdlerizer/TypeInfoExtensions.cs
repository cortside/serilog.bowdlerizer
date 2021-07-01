using System.Linq;
using System.Reflection;

namespace Serilog.Bowdlerizer {
    public static class TypeInfoExtensions {
        public static T GetCustomAttribute<T>(this TypeInfo typeInfo) {
            return typeInfo.GetCustomAttributes().OfType<T>().FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this PropertyInfo propertyInfo) {
            return propertyInfo.GetCustomAttributes().OfType<T>().FirstOrDefault();
        }
    }
}
