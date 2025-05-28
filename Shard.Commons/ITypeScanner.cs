using System.Reflection;

namespace Shard.Commons
{
    public interface ITypeScanner
    {
        /// <summary>
        /// Indicates if any types have been registered
        /// </summary>
        bool HasTypes { get; }

        /// <summary>
        /// Registers assemblies that will be scanned by the type scanner
        /// </summary>
        /// <param name="assemblies">A collection of assemblies</param>
        void Register(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Locates available types matching or implementing the provided type
        /// </summary>
        /// <param name="type">The type of object to locate</param>
        /// <returns>A list of located types</returns>
        IEnumerable<TypeInfo> Locate(Type type);

        /// <summary>
        /// Searches and transforms types on the fly. Return null from the mapper to exclude the type.
        /// </summary>
        /// <typeparam name="T">The projected type</typeparam>
        /// <param name="mapper">Receives the type and transforms it. Return null to exclude this type from the results.</param>
        /// <returns>A list of filtered projections</returns>
        IEnumerable<T> AdvancedSearch<T>(Func<TypeInfo, T> mapper) where T : class;
    }

    public static class TypeScannerExtensions
    {
        /// <summary>
        /// Registers assemblies that will be scanned by the type scanner
        /// </summary>
        /// <param name="assemblies">A collection of assemblies</param>
        /// <param name="assemblyNames">If true, will filter out system library names</param>
        public static void Register(this ITypeScanner typeScanner, IEnumerable<AssemblyName> assemblyNames, bool filterOutSystem = true)
        {
            var names = from asm in assemblyNames
                        where !asm.FullName.StartsAs("Microsoft.")
                        where !asm.FullName.StartsAs("System.")
                        where !asm.FullName.StartsAs("System,")
                        where !asm.FullName.StartsAs("Mono.")
                        where !asm.FullName.StartsAs("Mono,")
                        where !asm.FullName.StartsAs("mscorlib,")
                        select asm;

            var asms = names.Select(name =>
            {
                try { return Assembly.Load(name); }
                catch { return null; }
            }).Where(v => v != null);

            typeScanner.Register(asms);
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type
        /// </summary>
        /// <typeparam name="TType">The type of object to locate</typeparam>
        /// <returns>A list of located types</returns>
        public static IEnumerable<TypeInfo> Locate<TType>(this ITypeScanner typeScanner)
        {
            return typeScanner.Locate(typeof(TType));
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type and then creates them
        /// </summary>
        /// <typeparam name="T">THe type of object to locate</typeparam>
        /// <param name="parameters">A list of parameters to pass to the constructors</param>
        /// <returns>A list of located and instantiated types</returns>
        public static List<T> LocateAndCreate<T>(this ITypeScanner typeScanner, params object[] parameters) where T : class
        {
            return typeScanner.Locate(typeof(T)).InstantiateAs<T>(parameters);
        }

        /// <summary>
        /// Locates available types matching or implementing the provided type and then creates them
        /// </summary>
        /// <param name="type">The type of object to locate</param>
        /// <param name="parameters">A list of parameters to pass to the constructors</param>
        /// <returns>A list of located and instantiated types</returns>
        public static List<T> LocateAndCreate<T>(this ITypeScanner typeScanner, Type type, params object[] parameters) where T : class
        {
            return typeScanner.Locate(type).InstantiateAs<T>(parameters);
        }

        /// <summary>
        /// Locates available types according to custom criteria
        /// </summary>
        /// <param name="predicate">A predicate to filter the available types</param>
        /// <returns>A list of filtered types</returns>
        public static IEnumerable<TypeInfo> Search(this ITypeScanner typeScanner, Func<TypeInfo, bool> predicate)
        {
            return typeScanner.AdvancedSearch(v => (predicate(v)) ? v : null);
        }
    }
}
