using System.Reflection;

namespace Shard.Commons
{
    /// <summary>
    /// Scans loaded assemblies for requested types
    /// </summary>
    public class TypeScanner : ITypeScanner
    {
        public TypeScanner(Action<ITypeScanner> setup = null)
        {
            setup?.Invoke(this);
        }

        TypeInfo[] m_types = null;

        List<Assembly> m_assemblies = new List<Assembly>();

        /// <summary>
        /// Indicates if any types have been registered 
        /// </summary>
        public bool HasTypes { get { return GetTypes().Length > 0; } }

        TypeInfo[] GetTypes()
        {
            var types = m_types;
            if (types == null || types.Length == 0)
            {
                //Enumerate potentially usable types
                types = GetTypes(m_assemblies).OrderBy(v => v.Name).ThenBy(v => v.Namespace).ToArray();
                Interlocked.Exchange(ref m_types, types);
            }
            return types;
        }

        /// <summary>
        /// Scans for potentially usable types in an assembly and returns them
        /// </summary>
        /// <param name="from">The assembly to scan</param>
        /// <returns>An array of types discovered</returns>
        public static TypeInfo[] GetTypes(Assembly from) { return GetTypes(new[] { from }); }

        /// <summary>
        /// Scans for potentially usable types in the provided assemblies and returns them
        /// </summary>
        /// <param name="from">The assemblies to scan</param>
        /// <returns>An array of types discovered</returns>
        public static TypeInfo[] GetTypes(IEnumerable<Assembly> from)
        {
            //Enumerate potentially usable types
            var types = from.SelectMany(v =>
            {
                try { return v.DefinedTypes.ToArray(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).Select(t => t.GetTypeInfo()).ToArray(); }
                catch { return new TypeInfo[0]; }
            })
            .Where(v => !v.IsAbstract && v.IsClass)
            .ToArray();

            return types;
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type in a given type cache
        /// </summary>
        /// <typeparam name="TType">The type of object to locate</typeparam>
        /// <returns>A list of located types</returns>
        public static IEnumerable<TypeInfo> LocateIn<TType>(TypeInfo[] typeCache)
        {
            return LocateIn(typeCache, typeof(TType));
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type in a given type cache
        /// </summary>
        /// <param name="type">The type of object to locate</param>
        /// <returns>A list of located types</returns>
        public static IEnumerable<TypeInfo> LocateIn(TypeInfo[] typeCache, Type type)
        {
            if (typeCache == null) return new TypeInfo[0];
            else return typeCache.Where(type.GetTypeInfo().IsAssignableFrom);
        }

        /// <summary>
        /// Registers assemblies that will be scanned by the type scanner
        /// </summary>
        /// <param name="assemblies">A collection of assemblies</param>
        public void Register(IEnumerable<Assembly> assemblies)
        {
            lock (m_assemblies)
            {
                var hash = new HashSet<string>(m_assemblies.Select(v => v.FullName));
                foreach (var asm in assemblies)
                    if (!hash.Contains(asm.FullName))
                    {
                        m_assemblies.Add(asm);
                        hash.Add(asm.FullName);
                    }
                Interlocked.Exchange(ref m_types, null);
            }
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type
        /// </summary>
        /// <param name="type">The type of object to locate</param>
        /// <returns>A list of located types</returns>
        public IEnumerable<TypeInfo> Locate(Type type)
        {
            return GetTypes().Where(type.GetTypeInfo().IsAssignableFrom);
        }

        /// <summary>
        /// Searches and transforms types on the fly. Return null from the mapper to exclude the type.
        /// </summary>
        /// <typeparam name="T">The projected type</typeparam>
        /// <param name="mapper">Receives the type and transforms it. Return null to exclude this type from the results.</param>
        /// <returns>A list of filtered projections</returns>
        public IEnumerable<T> AdvancedSearch<T>(Func<TypeInfo, T> mapper) where T : class
        {
            return GetTypes().Select(mapper).Where(v => v != null);
        }
    }
}
