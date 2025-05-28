using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shard.Commons
{
    /// <summary>
    /// Provides reflection-related convenience extensions
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns the MemberInfo represented by a function expression of v=>v.Property
        /// </summary>
        /// <typeparam name="T">The object type the member is located on</typeparam>
        /// <param name="expression">An expression that accesses a member on the object type</param>
        /// <returns>The MemberInfo if resolved, or null if expression doesn't represent a member access expression</returns>
        public static MemberInfo GetMemberInfo<T, TRet>(this Expression<Func<T, TRet>> expression)
        {
            if (expression == null)
                return null;
            else if (expression.Body is MemberExpression)
                return (expression.Body as MemberExpression).Member;
            else if (expression.Body is UnaryExpression && (expression.Body as UnaryExpression).Operand is MemberExpression)
                return ((expression.Body as UnaryExpression).Operand as MemberExpression).Member;
            else
                return null;
        }

        /// <summary>
        /// Returns the MemberExpression represented by a function expression of v=>v.Property
        /// </summary>
        /// <param name="expression">An expression that accesses a member on the object type</param>
        /// <returns>The MemberExpression if resolved, or null if expression doesn't represent a member access expression</returns>
        public static MemberExpression GetMemberExpression(this LambdaExpression expression)
        {
            if (expression == null || expression.Body == null)
                return null;
            else if (expression.Body is MemberExpression)
                return (expression.Body as MemberExpression);
            else if (expression.Body is UnaryExpression && (expression.Body as UnaryExpression).Operand is MemberExpression)
                return (expression.Body as UnaryExpression).Operand as MemberExpression;
            else
                return null;
        }

        /// <summary>
        /// Returns a MethodCallExpression represented by a function expression of v=>v.Method()
        /// </summary>
        /// <param name="expression">An expression that accesses a method on the object type</param>
        /// <returns>The MethodCallExpression if resolved, or null if it doesn't represent a method call</returns>
        public static MethodCallExpression GetMethodCallExpression(this LambdaExpression expression)
        {
            if (expression == null || expression.Body == null)
                return null;
            else if (expression.Body is MethodCallExpression)
                return expression.Body as MethodCallExpression;
            else if (expression.Body is UnaryExpression && (expression.Body as UnaryExpression).Operand is MethodCallExpression)
                return (expression.Body as UnaryExpression).Operand as MethodCallExpression;
            else
                return null;
        }

        /// <summary>
        /// Returns the dotted name represented by a member expression, ie: v=>v.Object.Property will be converted to the string "Object.Property"
        /// </summary>
        /// <param name="expression">The member expression to convert</param>
        /// <returns>The dotted name string representing the named member, or null if MemberExpression is null</returns>
        public static string GetDottedName(this MemberExpression expression)
        {
            if (expression == null) return null;
            var sb = new StringBuilder(expression.Member.Name);
            var exp = expression.Expression as MemberExpression;
            while (exp != null)
            {
                sb.Insert(0, exp.Member.Name + ".");
                exp = exp.Expression as MemberExpression;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the dotted name represented by a method expression, ie: v=>v.Object.Method will be converted to the string "Object.Method"
        /// </summary>
        /// <param name="expression">The member expression to convert</param>
        /// <returns>The dotted name string representing the named member, or null if MemberExpression is null</returns>
        public static string GetDottedName(this MethodCallExpression expression)
        {
            if (expression == null) return null;
            var sb = new StringBuilder(expression.Method.Name);
            var exp = expression.Object as MemberExpression;
            while (exp != null)
            {
                sb.Insert(0, exp.Member.Name + ".");
                exp = exp.Expression as MemberExpression;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Resolves a member-or-method-access expression to a dotted name
        /// </summary>
        /// <typeparam name="T">The object type the member is located on</typeparam>
        /// <param name="expression">An expression that accesses a member or method on the object type</param>
        /// <returns>The dotted name string representing the name, or null if expression doesn't resolve</returns>
        public static string GetDottedName<T, TRet>(this Expression<Func<T, TRet>> expression)
        {
            var memEx = GetMemberExpression(expression);
            if (memEx != null) return GetDottedName(memEx);
            var methEx = GetMethodCallExpression(expression);
            if (methEx != null) return GetDottedName(methEx);
            return null;
        }

        /// <summary>
        /// Builds a member expression given a base type and dotted property name
        /// </summary>
        /// <param name="exp">The base expression (such as a ParameterExpression)</param>
        /// <param name="ownerType">The type of object that owns the dotted property name</param>
        /// <param name="propertyName">The dotted property name to access</param>
        /// <returns>A MemberExpression representing a member access</returns>
        public static MemberExpression GetMemberExpressionByDottedName(this Expression exp, Type ownerType, string propertyName)
        {
            if (propertyName == null || exp == null || ownerType == null) return null;
            return GetMemberExpressionByDottedName(exp, ownerType, propertyName, 0);
        }
        static MemberExpression GetMemberExpressionByDottedName(Expression exp, Type ownerType, string propertyName, int startAt)
        {
            var indexOf = propertyName.IndexOf('.', startAt);
            string thisPart;
            if (indexOf == -1) thisPart = startAt == 0 ? propertyName : propertyName.Substring(startAt);
            else thisPart = propertyName.Substring(startAt, indexOf - startAt);

            var pi = ownerType.GetRuntimeProperty(thisPart);
            if (pi == null) return null;

            var memEx = Expression.Property(exp, pi);
            if (indexOf == -1) return memEx;
            else return GetMemberExpressionByDottedName(memEx, pi.PropertyType, propertyName, indexOf + 1);
        }

        /// <summary>
        /// Returns the property or field type represented by the MemberInfo
        /// </summary>
        /// <param name="member">The MemberInfo to retrieve the property/field type from</param>
        /// <returns>The resolved type, or null if MemberInfo isn't a PropertyInfo or FieldInfo instance</returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is PropertyInfo) return (member as PropertyInfo).PropertyType;
            else if (member is FieldInfo) return (member as FieldInfo).FieldType;
            else return null;
        }


        /// <summary>
        /// Returns the value of a property or field represented by the MemberInfo
        /// </summary>
        /// <param name="member">The MemberInfo to retrieve the property/field value from</param>
        /// <returns>The retrieved value, or null if MemberInfo isn't a PropertyInfo or FieldInfo instance</returns>
        public static object GetReflectedValue(this MemberInfo member, object obj)
        {
            if (member is PropertyInfo) return (member as PropertyInfo).GetValue(obj, null);
            else if (member is FieldInfo) return (member as FieldInfo).GetValue(obj);
            else return null;
        }

        /// <summary>
        /// Returns a value from an object given a dotted member string
        /// </summary>
        /// <param name="obj">The object to retrieve the value from</param>
        /// <param name="memberName">A dotted member access string representing the member to retrieve</param>
        /// <returns>The retrieved value, or null if memberName was unable to be resolved</returns>
        public static object GetReflectedValue(this object obj, string memberName)
        {
            if (obj == null || memberName == null) return null;
            return GetReflectedValue(obj, obj.GetType(), memberName, 0);
        }
        static object GetReflectedValue(this object obj, Type ownerType, string memberName, int startAt)
        {
            var indexOf = memberName.IndexOf('.', startAt);
            string thisPart;
            if (indexOf == -1) thisPart = startAt == 0 ? memberName : memberName.Substring(startAt);
            else thisPart = memberName.Substring(startAt, indexOf - startAt);

            MemberInfo member = ownerType.GetRuntimeProperty(thisPart);
            if (member == null) member = ownerType.GetRuntimeField(thisPart);
            var retVal = GetReflectedValue(member, obj);

            if (indexOf == -1 || retVal == null) return retVal;
            else return GetReflectedValue(retVal, GetMemberType(member), memberName, indexOf + 1);
        }

        /// <summary>
        /// Gets a value from an object, then attempts to format it. Similar to the old DataBinder.Eval() call. If the formatter is null or blank, the raw value string is returned.
        /// </summary>
        /// <param name="obj">The object to retrieve a value from</param>
        /// <param name="memberName">The member to retrieve</param>
        /// <param name="formatString">The optional formatting string to use (must include the positional argument)</param>
        /// <returns>A string representation; never null</returns>
        public static string Eval(this object obj, string memberName, string formatString = null)
        {
            var val = obj.GetReflectedValue(memberName);
            if (formatString.IsNullOrBlank()) return val?.ToString() ?? "";
            else return formatString.FormatWith(val) ?? "";
        }
    }
}
