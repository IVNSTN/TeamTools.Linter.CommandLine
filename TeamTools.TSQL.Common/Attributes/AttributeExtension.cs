using System;

namespace TeamTools.Common.Linting
{
    public static class AttributeExtension
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(TAttribute), false);
            if (attrs.Length > 0 && attrs[0] is TAttribute att)
            {
                return valueSelector(att);
            }

            return default;
        }
    }
}
