namespace Shard.Commons
{
    public struct ShortGuid : IComparable<ShortGuid>, IComparable<Guid>, IEquatable<ShortGuid>, IEquatable<Guid>
    {
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        public ShortGuid(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; private set; }

        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        /// <summary>
        /// Tests if a string is a valid store ID
        /// </summary>
        /// <param name="value">The string to test</param>
        /// <returns>True if valid, false if invalid(includes null/empty)</returns>
        public static bool IsValid(string value)
        {
            return TryParse(value).HasValue;
        }

        /// <summary>
        /// If the string is valid, returns a ShortGuid representing it. If not, returns null.
        /// </summary>
        /// <param name="value">The ShortGuid string, or full guid string, to convert</param>
        /// <returns>A ShortGuid or null</returns>
        public static ShortGuid? TryParse(string value)
        {
            if (value.IsNullOrBlank()) return null;
            if (value == "0") return Empty;
            Guid? guid = null;
            if (Guid.TryParse(value, out var g)) guid = g;
            if (guid == null)
            {
                var bytes = Hex32.Decode(value, nullOnError: true);
                if (bytes == null || bytes.Length != 16) return null;
                guid = new Guid(bytes);
            }
            return new ShortGuid(guid.Value);
        }

        //Basic value semantics
        public override string ToString()
        {
            if (Value == Guid.Empty) return "0";
            else return Hex32.Encode(Value.ToByteArray());
        }

        public override int GetHashCode() { return Value.GetHashCode(); }

        public bool Equals(Guid other) { return other == Value; }
        public bool Equals(ShortGuid other) { return other.Value == Value; }
        public override bool Equals(object obj) { return (obj is ShortGuid && Equals((ShortGuid)obj)) || (obj is Guid && Equals((Guid)obj)); }

        public int CompareTo(ShortGuid other) { return Value.CompareTo(other.Value); }
        public int CompareTo(Guid other) { return Value.CompareTo(other); }

        public static bool operator ==(ShortGuid x, ShortGuid y) { return ReferenceEquals(x, y) || (!ReferenceEquals(x, null) && x.Equals(y)); }
        public static bool operator !=(ShortGuid x, ShortGuid y) { return !(x == y); }

        public static implicit operator ShortGuid(Guid val) { return new ShortGuid(val); }
        public static implicit operator Guid(ShortGuid val) { return val.Value; }
    }
}
