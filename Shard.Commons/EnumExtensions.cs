using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Shard.Commons
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Helper to return the values of an enum as a typed array.
        /// </summary>
        /// <typeparam name="T">The type of enum to retrieve the values from</typeparam>
        /// <returns>An array containing the properly typed enum values</returns>
        public static T[] Values<T>()
        {
            var vals = Enum.GetValues(typeof(T));
            var typed = new T[vals.Length];
            for (var i = 0; i < vals.Length; i++) typed[i] = (T)vals.GetValue(i);
            return typed;
        }

        /// <summary>
        /// Returns the Display Attribute name for the provided enum value, when available
        /// </summary>
        /// <param name="value">The enum value to get the display value for</param>
        /// <returns>The display value, or the value's name if one isn't set</returns>
        public static string ToDisplay(this Enum value)
        {
            if (value == null) return "";
            var type = value.GetType();
            var name = value.ToString();
            return type.GetRuntimeField(name)?.GetCustomAttribute<DisplayAttribute>()?.Name ?? name;
        }
    }
}
