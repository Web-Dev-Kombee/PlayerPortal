using System.Reflection;

namespace Shard.Commons
{
    /// <summary>
    /// Provides object-related convenience extensions
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Tests if an object is null, if not - executes the nominated function and returns it's value
        /// </summary>
        /// <typeparam name="T">The type of object to check</typeparam>
        /// <typeparam name="TResult">The type of object returned by the function</typeparam>
        /// <param name="obj">The object to test if null</param>
        /// <param name="callback">The function to invoke on the non-null object</param>
        /// <returns>The result of the function if not null, otherwise the default value for TResult</returns>
        public static TResult IfNotNull<T, TResult>(this T obj, Func<T, TResult> callback)
        {
            if (obj == null) return default(TResult);
            else return callback(obj);
        }

        /// <summary>
        /// Tests if an object is null, if not - executes the nominated action
        /// </summary>
        /// <typeparam name="T">The type of object to check</typeparam>
        /// <param name="obj">The object to test if null</param>
        /// <param name="callback">The callback to invoke if the object is not null</param>
        public static void IfNotNull<T>(this T obj, Action<T> callback)
        {
            if (obj != null) callback(obj);
        }

        /// <summary>
        /// Executes an action against an object, then returns the object for chaining
        /// </summary>
        /// <typeparam name="T">The type of object</typeparam>
        /// <param name="obj">The object to use</param>
        /// <param name="callback">The callback to invoke against the object</param>
        public static T Do<T>(this T obj, Action<T> callback)
        {
            callback(obj);
            return obj;
        }

        /// <summary>
        /// Executes a function against an object to alter it, returning a new object
        /// </summary>
        /// <typeparam name="TIn">The type of object to alter</typeparam>
        /// <typeparam name="TOut">The type of object to return</typeparam>
        /// <param name="obj">The object to use</param>
        /// <param name="callback">The callback to invoke against the object</param>
        public static TOut Transform<TIn, TOut>(this TIn obj, Func<TIn, TOut> callback)
        {
            return callback(obj);
        }

        /// <summary>
        /// Converts an objects immediate, public members into a dictionary via reflection
        /// </summary>
        /// <typeparam name="T">The type of object to convert</typeparam>
        /// <param name="obj">The object to convert</param>
        /// <returns>A dictionary of key/values. Null objects produce empty dictionaries.</returns>
        public static IDictionary<string, object> ToDictionary<T>(this T obj)
        {
            var ret = new Dictionary<string, object>();
            if (obj == null) return ret;
            foreach (var member in obj.GetType().GetTypeInfo().DeclaredMembers)
                ret[member.Name] = member.GetReflectedValue(obj);
            return ret;
        }
    }
}
