using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Convience shorthand for Acivator.CreateInstance
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <param name="parameters">The parameters to pass the constructor, if any</param>
        /// <returns>The new object</returns>
        public static object Instantiate(this Type type, params object[] parameters)
        {
            return Activator.CreateInstance(type, parameters);
        }

        /// <summary>
        /// Convience shorthand for Acivator.CreateInstance, but callable on a TypeInfo
        /// </summary>
        /// <param name="typeinfo">The typeinfo to create from</param>
        /// <param name="parameters">The parameters to pass the constructor, if any</param>
        /// <returns>The new object</returns>
        public static object Instantiate(this TypeInfo typeinfo, params object[] parameters)
        {
            return Activator.CreateInstance(typeinfo.AsType(), parameters);
        }

        /// <summary>
        /// Creates instances of objects for a provided type list.
        /// </summary>
        /// <typeparam name="T">The type of object to materialise the types as</typeparam>
        /// <param name="types">The list of types to materialise</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the objects</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateAs<T>(this IEnumerable<Type> types, params object[] parameters) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var extender in types)
                objects.Add(extender.InstantiateAs<T>(parameters));
            return objects;
        }

        /// <summary>
        /// Creates instances of objects for a provided typeinfo list.
        /// </summary>
        /// <typeparam name="T">The type of object to materialise the types as</typeparam>
        /// <param name="types">The list of types to materialise</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the objects</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateAs<T>(this IEnumerable<TypeInfo> types, params object[] parameters) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var extender in types)
                objects.Add(extender.InstantiateAs<T>(parameters));
            return objects;
        }

        /// <summary>
        /// Creates an instance of an object for a provided type.
        /// </summary>
        /// <typeparam name="T">The type of object to materialise the type as</typeparam>
        /// <param name="type">The type to materialise</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the object</param>
        /// <returns>The materialised object matching T</returns>
        public static T InstantiateAs<T>(this Type type, params object[] parameters)
        {
            var obj = Activator.CreateInstance(type, parameters);
            if (obj == null) return default(T);
            else return (T)obj;
        }

        /// <summary>
        /// Creates an instance of an object for a provided typeinfo.
        /// </summary>
        /// <typeparam name="T">The type of object to materialise the type as</typeparam>
        /// <param name="type">The type to materialise</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the object</param>
        /// <returns>The materialised object matching T</returns>
        public static T InstantiateAs<T>(this TypeInfo type, params object[] parameters)
        {
            return InstantiateAs<T>(type.AsType(), parameters);
        }

        /// <summary>
        /// Creates an instance of an object for a provided type by calling a static method on it.
        /// </summary>
        /// <typeparam name="T">The type of objects to create</typeparam>
        /// <param name="types">The list of types to materialise</param>
        /// <param name="staticMethod">The static method to invoke</param>
        /// <param name="parameters">Any arguments the method requires</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateUsingStatic<T>(this IEnumerable<Type> types, string staticMethod, params object[] parameters) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var extender in types)
                objects.Add(extender.InvokeStaticMember(staticMethod, parameters) as T);
            return objects;
        }

        /// <summary>
        /// Executes a static member on a given type
        /// </summary>
        /// <typeparam name="T">The type of objects to create</typeparam>
        /// <param name="types">The list of types to materialise</param>
        /// <param name="staticMethod">The static method to invoke</param>
        /// <param name="parameters">Any arguments the method requires</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateUsingStatic<T>(this IEnumerable<TypeInfo> types, string staticMethod, params object[] parameters) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var extender in types)
                objects.Add(extender.InvokeStaticMember(staticMethod, parameters) as T);
            return objects;
        }

        /// <summary>
        /// Creates an instance of an object for a provided type by calling a static method on it.
        /// </summary>
        /// <param name="type">The type to materialise</param>
        /// <param name="staticMethod">The static method to invoke</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the object</param>
        /// <returns>The result of the call</returns>
        public static object InvokeStaticMember(this Type type, string staticMethod, params object[] parameters)
        {
            return type.GetTypeInfo().InvokeStaticMember(staticMethod, parameters);
        }

        /// <summary>
        /// Creates an instance of an object for a provided type by calling a static method on it.
        /// </summary>
        /// <param name="type">The type to materialise</param>
        /// <param name="staticMethod">The static method to invoke</param>
        /// <param name="parameters">Optional arguments to pass to the constructor when creating the object</param>
        /// <returns>The materialised object matching T</returns>
        public static object InvokeStaticMember(this TypeInfo type, string staticMethod, params object[] parameters)
        {
            List<Type> typesList = new List<Type>();
            foreach (var o in parameters) typesList.Add(o.GetType());
            MethodInfo method = null;
            while (type != null && method == null)
            {
                method = type.DeclaredMethods.FirstOrDefault(v => v.Name == staticMethod && v.IsStatic &&
                    v.GetParameters().Select(x => x.ParameterType).SequenceEqual(typesList, TypeComparer.Instance));
                type = type.BaseType?.GetTypeInfo();
            }
            if (method == null) throw new MissingMemberException($"The static method {staticMethod} could not be found on {type}");
            return method.Invoke(null, parameters);
        }

        /// <summary>
        /// Accesses a static property or field on a given type
        /// </summary>
        /// <param name="type">The type containing the method</param>
        /// <param name="staticProperty">The static property/field to invoke</param>
        /// <returns>The result of the call</returns>
        public static object InvokeStaticProperty(this Type type, string staticProperty)
        {
            var target = type.GetTypeInfo();
            MemberInfo property = null;
            while (target != null && property == null)
            {
                property = target.DeclaredProperties.FirstOrDefault(v => v.CanRead && v.GetMethod.IsStatic && v.Name == staticProperty);
                if (property == null) property = target.DeclaredFields.FirstOrDefault(v => v.IsStatic && v.Name == staticProperty);
                target = target.BaseType?.GetTypeInfo();
            }
            if (property == null) throw new MissingMemberException($"The static property {staticProperty} could not be found on {type}");
            return property is PropertyInfo ? (property as PropertyInfo).GetValue(null) : (property as FieldInfo).GetValue(null);
        }

        /// <summary>
        /// Creates objects using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default. It will be invoked for every object.
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateUsing<T>(this IEnumerable<Type> types, Func<Type, Result<object>> typeResolver) where T : class
        {
            List<T> objects = new List<T>();
            var cache = new Dictionary<Type, List<Tuple<ConstructorInfo, ParameterInfo[]>>>();
            foreach (var type in types)
                objects.Add(InstantiateUsingCache(type, typeResolver, cache) as T);
            return objects;
        }

        /// <summary>
        /// Creates an object using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static T InstantiateUsing<T>(this Type type, Func<Type, Result<object>> typeResolver) where T : class
        {
            return InstantiateUsingCache(type, typeResolver, null) as T;
        }

        /// <summary>
        /// Creates an object using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static object InstantiateUsing(this Type type, Func<Type, Result<object>> typeResolver)
        {
            return InstantiateUsingCache(type, typeResolver, null);
        }

        /// <summary>
        /// Creates objects using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default. It will be invoked for every object.
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateUsing<T>(this IEnumerable<TypeInfo> types, Func<Type, Result<object>> typeResolver) where T : class
        {
            List<T> objects = new List<T>();
            var cache = new Dictionary<Type, List<Tuple<ConstructorInfo, ParameterInfo[]>>>();
            foreach (var type in types)
                objects.Add(InstantiateUsingCache(type.AsType(), typeResolver, cache) as T);
            return objects;
        }

        /// <summary>
        /// Creates objects using a type dictionary. It will start with the constructor with the most arguments and work backwards to the shortest/default. It will be invoked for every object.
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeMap">The dictionary to resolve required types. If an exact type match isn't present, the constructor will be ignored.</param>
        /// <returns>A list of materialised objects matching T</returns>
        public static List<T> InstantiateUsing<T>(this IEnumerable<TypeInfo> types, IDictionary<Type, object> typeMap) where T : class
        {
            List<T> objects = new List<T>();
            var cache = new Dictionary<Type, List<Tuple<ConstructorInfo, ParameterInfo[]>>>();
            foreach (var type in types)
                objects.Add(InstantiateUsingCache(type.AsType(), t => typeMap.GetValue(t), cache) as T);
            return objects;
        }

        /// <summary>
        /// Creates an object using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static T InstantiateUsing<T>(this TypeInfo type, Func<Type, Result<object>> typeResolver) where T : class
        {
            return InstantiateUsingCache(type.AsType(), typeResolver, null) as T;
        }

        /// <summary>
        /// Creates an object using a type dictionary. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <typeparam name="T">The type to cast the new object to</typeparam>
        /// <param name="type">The type to create</param>
        /// <param name="typeMap">The dictionary to resolve required types. If an exact type match isn't present, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static T InstantiateUsing<T>(this TypeInfo type, IDictionary<Type, object> typeMap) where T : class
        {
            return InstantiateUsingCache(type.AsType(), t => typeMap.GetValue(t), null) as T;
        }

        /// <summary>
        /// Creates an object using a type resolver. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <param name="typeResolver">The method to resolve a required type. If failure is returned, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static object InstantiateUsing(this TypeInfo type, Func<Type, Result<object>> typeResolver)
        {
            return InstantiateUsingCache(type.AsType(), typeResolver, null);
        }

        /// <summary>
        /// Creates an object using a type dictionary. It will start with the constructor with the most arguments and work backwards to the shortest/default
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <param name="typeMap">The dictionary to resolve required types. If an exact type match isn't present, the constructor will be ignored.</param>
        /// <returns>The constructed object</returns>
        public static object InstantiateUsing(this TypeInfo type, IDictionary<Type, object> typeMap)
        {
            return InstantiateUsingCache(type.AsType(), t => typeMap.GetValue(t), null);
        }

        static object InstantiateUsingCache(Type type, Func<Type, Result<object>> typeResolver, Dictionary<Type, List<Tuple<ConstructorInfo, ParameterInfo[]>>> cache)
        {
            var constructors = cache?.GetValue(type);
            if (constructors == null)
            {
                constructors = type.GetTypeInfo().DeclaredConstructors
                                .Where(v => !v.IsStatic)
                                .Select(v => Tuple.Create(v, v.GetParameters()))
                                .OrderByDescending(v => v.Item2.Length)
                                .ToList();
                if (cache != null)
                    cache[type] = constructors;
            }

            var args = new List<object>();
            var prevArgs = new Dictionary<Type, object>();
            foreach (var ctor in constructors)
            {
                args.Clear();
                var badCtor = false;
                foreach (var arg in ctor.Item2)
                {
                    if (prevArgs.ContainsKey(arg.ParameterType)) args.Add(prevArgs[arg.ParameterType]);
                    else
                    {
                        var res = typeResolver(arg.ParameterType) ?? false;
                        if (!res) { badCtor = true; break; }
                        prevArgs[arg.ParameterType] = res.Data;
                        args.Add(res.Data);
                    }
                }
                if (badCtor) continue;

                if (cache != null && constructors.Count > 1)
                {
                    //Update cache to only try this one for future types, as this is the one we successfully matched
                    cache[type] = new[] { ctor }.ToList();
                }

                return ctor.Item1.Invoke(args.ToArray());
            }

            //Unable to match a constructor
            throw new MissingMemberException($"Unable to locate a suitable constructor for {type}");
        }

        class TypeComparer : IEqualityComparer<Type>
        {
            internal static readonly TypeComparer Instance = new TypeComparer();

            public bool Equals(Type x, Type y)
            {
                return (x == null && y == null) || (x != null && y != null && x.GetTypeInfo().IsAssignableFrom(y.GetTypeInfo()));
            }

            public int GetHashCode(Type obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }

        /// <summary>
        /// Retrieves all implemented interfaces on this type, and all inherited types. Results are de-duplicated.
        /// </summary>
        /// <param name="type">The type to scan</param>
        /// <returns>A list of all implemented + inherited interfaces</returns>
        public static IEnumerable<Type> GetAllInterfaces(this Type type) => GetAllInterfaces(type.GetTypeInfo());

        /// <summary>
        /// Retrieves all implemented interfaces on this type, and all inherited types. Results are de-duplicated.
        /// </summary>
        /// <param name="typeInfo">The type to scan</param>
        /// <returns>A list of all implemented + inherited interfaces</returns>
        public static IEnumerable<Type> GetAllInterfaces(this TypeInfo typeInfo)
        {
            HashSet<Type> interfaces = new HashSet<Type>();
            GetAllInterfaces(typeInfo, interfaces);
            return interfaces;
        }

        static void GetAllInterfaces(TypeInfo typeInfo, HashSet<Type> interfaces)
        {
            if (typeInfo != null)
            {
                foreach (var iface in typeInfo.ImplementedInterfaces)
                    interfaces.Add(iface);

                if (typeInfo.BaseType != null)
                    GetAllInterfaces(typeInfo.BaseType.GetTypeInfo(), interfaces);
            }
        }
    }
}
