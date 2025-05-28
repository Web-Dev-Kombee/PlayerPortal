using System.Globalization;

namespace Shard.Commons
{
    /// <summary>
    /// Provides string-related parsing helpers
    /// </summary>
    public static class StringParsing
    {
        /// <summary>
        /// Converts a provided string to a Byte value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Integer</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable byte value if it succeeded</returns>
        public static byte? ToByte(this string input, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            byte val;
            if (!byte.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to an Int16 value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Integer</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable short value if it succeeded</returns>
        public static short? ToShort(this string input, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            short val;
            if (!short.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to an Int32 value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Integer</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable int value if it succeeded</returns>
        public static int? ToInt(this string input, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            int val;
            if (!int.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to an Int64 value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Integer</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable long value if it succeeded</returns>
        public static long? ToLong(this string input, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            long val;
            if (!long.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a Decimal value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Number</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable decimal value if it succeeded</returns>
        public static decimal? ToDecimal(this string input, NumberStyles style = NumberStyles.Number, IFormatProvider provider = null)
        {
            decimal val;
            if (!decimal.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a Byte value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The number style to use, defaults to Number</param>
        /// <param name="provider">The provider to use. If null, defaults to NumberFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable double value if it succeeded</returns>
        public static double? ToDouble(this string input, NumberStyles style = NumberStyles.Number, IFormatProvider provider = null)
        {
            double val;
            if (!double.TryParse(input, style, provider ?? NumberFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a TimeSpan value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="provider">The provider to use. If null, defaults to DateTimeFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable TimeSpan value if it succeeded</returns>
        public static TimeSpan? ToTimeSpan(this string input, IFormatProvider provider = null)
        {
            TimeSpan val;
            if (!TimeSpan.TryParse(input, provider ?? DateTimeFormatInfo.CurrentInfo, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a DateTime value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The datetime style to use, defaults to AllowWhiteSpaces</param>
        /// <param name="provider">The provider to use. If null, defaults to DateTimeFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable DateTime value if it succeeded</returns>
        public static DateTime? ToDateTime(this string input, DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces, IFormatProvider provider = null)
        {
            DateTime val;
            if (!DateTime.TryParse(input, provider ?? DateTimeFormatInfo.CurrentInfo, style, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a DateTimeOffset value
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <param name="style">The datetime style to use, defaults to AllowWhiteSpaces</param>
        /// <param name="provider">The provider to use. If null, defaults to DateTimeFormatInfo.CurrentInfo</param>
        /// <returns>Null if conversion failed, or a nullable DateTimeOffset value if it succeeded</returns>
        public static DateTimeOffset? ToDateTimeOffset(this string input, DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces, IFormatProvider provider = null)
        {
            DateTimeOffset val;
            if (!DateTimeOffset.TryParse(input, provider ?? DateTimeFormatInfo.CurrentInfo, style, out val)) return null;
            return val;
        }

        /// <summary>
        /// Converts a provided string to a Bool value
        /// </summary>
        /// <param name="input">The string to convert. Understands the values bool.TrueString, bool.FalseString, 0, 1, -1. Whitespace and case is ignored. Compare is invariant culture.</param>
        /// <returns>Null if conversion failed, or a nullable bool value if it succeeded</returns>
        public static bool? ToBool(this string input)
        {
            var trimmed = input.TrimWhitespace();
            if (trimmed == null || trimmed.Length == 0) return null;
            else if (input == "0" || input.Is(bool.FalseString)) return false;
            else if (input == "1" || input == "-1" || input.Is(bool.TrueString)) return true;
            else return null;
        }

        /// <summary>
        /// Converts a provided string to a Guid value
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>Null if conversion failed, or a nullable Guid value if it succeeded.</returns>
        public static Guid? ToGuid(this string input)
        {
            return ShortGuid.TryParse(input); //ShortGuid will also parse normal guid strings
        }

        /// <summary>
        /// Converts a provided string to a ShortGuid value
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>Null if conversion failed, or a nullable ShortGuid value if it succeeded.</returns>
        public static ShortGuid? ToShortGuid(this string input)
        {
            return ShortGuid.TryParse(input);
        }

        /// <summary>
        /// Converts a provided string to a specified Enum value
        /// </summary>
        /// <typeparam name="T">The enum type to convert the string to</typeparam>
        /// <param name="input">The string to convert. Accepts both the numerical version and the string version of an enum value. Attempts name match first.</param>
        /// <param name="ignoreCase">Indicates if the name should be matched in a case insensitive manner.</param>
        /// <returns>Null if conversion failed, or a nullable enum value of type T.</returns>
        public static T? ToEnum<T>(this string input, bool ignoreCase = true) where T : struct
        {
            try { return (T)Enum.Parse(typeof(T), input, ignoreCase); }
            catch
            {
                var asInt = input.ToInt();
                if (asInt == null) return null;
                try { return (T)Enum.ToObject(typeof(T), asInt.Value); }
                catch { return null; }
            }
        }
    }
}
