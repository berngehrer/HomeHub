using System;
using System.Linq;

namespace Openweathermap.net
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class FieldValueAttribute : Attribute
    {
        public FieldValueAttribute(object value)
        {
            Value = value;
        }
        public object Value { get; }
        public override string ToString() => Value?.ToString();
    }


    internal static class FieldValueExtensions
    {
        public static object GetValue(this Enum e) => e.GetAttribute<FieldValueAttribute>()?.Value;
    }
}