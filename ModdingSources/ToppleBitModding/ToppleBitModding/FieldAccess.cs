using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ToppleBitModding
{
    public static class FieldAccess
    {
        private static readonly Dictionary<(Type, string), FieldInfo> _fieldCache
            = new Dictionary<(Type, string), FieldInfo>();

        private static FieldInfo GetField(Type type, string fieldName)
        {
            var key = (type, fieldName);

            if (_fieldCache.TryGetValue(key, out var field))
                return field;

            field = type.GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            if (field == null)
                throw new MissingFieldException(type.FullName, fieldName);

            _fieldCache[key] = field;
            return field;
        }

        public static T Get<T>(object instance, string fieldName)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var field = GetField(instance.GetType(), fieldName);
            return (T)field.GetValue(instance);
        }

        public static void Set<T>(object instance, string fieldName, T value)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var field = GetField(instance.GetType(), fieldName);
            field.SetValue(instance, value);
        }
    }
}
