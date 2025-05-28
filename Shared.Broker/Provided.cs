using Shared.Broker.Internal;

namespace Shared.Broker
{
    public class Provided<T>
    {
        private Provided(bool success, T result) { Success = success; Result = result; }
        public Provided(T result) : this(true, result) { }

        public bool Success { get; private set; }
        public T Result { get; private set; }

        public static implicit operator Provided<T>(bool res)
        {
            return new Provided<T>(res, default(T));
        }

        public static implicit operator Provided<T>(T data)
        {
            return new Provided<T>(true, data);
        }

        public static implicit operator Provided<T>(ProvidedIntent res)
        {
            return new Provided<T>(res.Result, ReferenceEquals(res.Data, null) ? default(T) : (T)res.Data);
        }

        public static implicit operator bool(Provided<T> prov)
        {
            return prov?.Success ?? false;
        }
    }

    public static class Provided
    {
        public static ProvidedIntent NoResult { get { return new ProvidedIntent(false, null); } }
        public static ProvidedIntent Result(object data) { return new ProvidedIntent(true, data); }
    }
}
