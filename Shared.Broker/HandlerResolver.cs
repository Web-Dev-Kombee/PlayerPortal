using Shard.Commons;
using Shared.Broker.Internal;
using System.Reflection;

namespace Shared.Broker
{
    public class HandlerResolver
    {
        public HandlerResolver()
        {
        }

        readonly Type m_singleType = typeof(IBrokeredSingleMapping<>);
        readonly Type m_manyType = typeof(IBrokeredManyMapping<>);

        Dictionary<Type, object> m_singleHandlers = new Dictionary<Type, object>();
        Dictionary<Type, List<object>> m_manyHandlers = new Dictionary<Type, List<object>>();

        public void LoadFrom(IEnumerable<Type> types, Action<ITypeMapper> conArgProvider = null)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            Load(types.Select(v => Filter(v.GetTypeInfo())).Where(v => v != null), conArgProvider);
        }

        public void LoadFrom(IEnumerable<TypeInfo> types, Action<ITypeMapper> conArgProvider = null)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            Load(types.Select(v => Filter(v)).Where(v => v != null), conArgProvider);
        }

        public void LoadAll(ITypeScanner typeScanner, Action<ITypeMapper> conArgProvider = null)
        {
            if (typeScanner == null) throw new ArgumentNullException(nameof(typeScanner));
            Load(typeScanner.AdvancedSearch(Filter), conArgProvider);
        }

        FilteredType Filter(TypeInfo v)
        {
            var filtered = new FilteredType { Handler = v };
            if (v.IsClass && !v.IsAbstract)
                foreach (var face in v.GetAllInterfaces().Where(face => face.IsConstructedGenericType))
                {
                    var def = face.GetGenericTypeDefinition();
                    if (def == m_singleType) filtered.Interfaces.Add(new Interfaces { Single = true, DataType = face.GenericTypeArguments[0] });
                    else if (def == m_manyType) filtered.Interfaces.Add(new Interfaces { Single = false, DataType = face.GenericTypeArguments[0] });
                }
            return filtered.Interfaces.Count == 0 ? null : filtered;
        }

        void Load(IEnumerable<FilteredType> types, Action<ITypeMapper> conArgProvider = null)
        {
            var typeMap = new TypeMap();
            conArgProvider?.Invoke(typeMap);

            foreach (var found in types)
            {
                var handler = found.Handler.InstantiateUsing(typeMap.Resolver);
                Add(handler, found.Interfaces);
            }
        }

        void Load<T>(IBrokeredSingleMapping<T> handler) { m_singleHandlers[typeof(T)] = handler; }
        void Load<T>(IBrokeredManyMapping<T> handler)
        {
            var t = typeof(T);
            if (m_manyHandlers.ContainsKey(t)) m_manyHandlers[t].Add(handler);
            else (m_manyHandlers[t] = new List<object>()).Add(handler);
        }

        public void Add(params object[] handlers) { Add(handlers as IEnumerable<object>); }

        public void Add(IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                if (handler == null) continue;
                var handType = handler.GetType().GetTypeInfo();
                var filtered = Filter(handType);

                if (filtered == null || filtered.Interfaces.Count == 0)
                    throw new ArgumentOutOfRangeException($"The provided handler {handType.FullName} doesn't implement a Broker Handler interface.");

                Add(handler, filtered.Interfaces);
            }
        }

        void Add(object handler, List<Interfaces> ifaces)
        {
            foreach (var iface in ifaces)
            {
                if (iface.Single)
                {
                    if (m_singleHandlers.ContainsKey(iface.DataType))
                        throw new ArgumentOutOfRangeException($"The provided handler {handler.GetType().FullName} handles {iface.DataType} - but this type has already been registered by {m_singleHandlers[iface.DataType].GetType().FullName}. Use Clear() if this is intentional before adding this type.");

                    m_singleHandlers[iface.DataType] = handler;
                }
                else if (m_manyHandlers.ContainsKey(iface.DataType)) m_manyHandlers[iface.DataType].Add(handler);
                else (m_manyHandlers[iface.DataType] = new List<object>()).Add(handler);
            }
        }

        public void Clear<T>()
        {
            var t = typeof(T);
            if (m_manyHandlers.ContainsKey(t)) m_manyHandlers.Remove(t);
            if (m_singleHandlers.ContainsKey(t)) m_singleHandlers.Remove(t);
        }

        public object SingleResolver(Type messageType)
        {
            return m_singleHandlers.GetValue(messageType);
        }

        public IEnumerable<object> ManyResolver(Type messageType)
        {
            return m_manyHandlers.GetValue(messageType);
        }

        class TypeMap : ITypeMapper
        {
            Dictionary<Type, object> m_map = new Dictionary<Type, object>();
            List<Tuple<Func<Type, object>, bool>> m_dynamic = new List<Tuple<Func<Type, object>, bool>>(); //bool = ignoreEnumerabls

            public ITypeMapper Add(Type t, object objForT)
            {
                m_map[t] = objForT;
                return this;
            }

            public ITypeMapper Add<T>(T objForT)
            {
                m_map[typeof(T)] = objForT;
                return this;
            }

            public ITypeMapper AddDynamic(Func<Type, object> getter, bool ignoreEnumerables)
            {
                m_dynamic.Add(Tuple.Create(getter, ignoreEnumerables));
                return this;
            }

            internal Result<object> Resolver(Type arg)
            {
                if (m_map.ContainsKey(arg))
                {
                    return m_map[arg];
                }
                else if (m_dynamic.Count > 0)
                {
                    var enumerableType = typeof(System.Collections.IEnumerable).GetTypeInfo();
                    var isEnumerable = arg.GetTypeInfo().GetAllInterfaces().Any(i => i.GetTypeInfo().IsAssignableFrom(enumerableType));
                    foreach (var dyn in m_dynamic)
                    {
                        if (isEnumerable && dyn.Item2) continue;
                        var obj = dyn.Item1(arg);
                        if (obj != null) return obj;
                    }
                }

                return false;
            }
        }

        class FilteredType
        {
            public TypeInfo Handler { get; set; }
            public List<Interfaces> Interfaces { get; set; } = new List<HandlerResolver.Interfaces>();
        }

        class Interfaces
        {
            public bool Single { get; set; }
            public Type DataType { get; set; }
        }
    }
}
