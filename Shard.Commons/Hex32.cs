using System.Text;

namespace Shard.Commons
{
    /// <summary>
    /// Encodes data using a URI + case-insensitive file-safe scheme.
    /// </summary>
    public static class Hex32
    {
        static readonly char[] s_base32;
        static readonly int[] s_base32dec;

        static Hex32()
        {
            s_base32 = new char[32];
            s_base32dec = new int[256];
            for (var idx = 0; idx < 256; idx++) s_base32dec[idx] = -1; //Invalid char
            char c;
            int i = 0;
            for (c = '0'; c <= '9'; c++, i++) { s_base32[i] = c; s_base32dec[(int)c] = i; }
            for (c = 'a'; c <= 'v'; c++, i++) { s_base32[i] = c; s_base32dec[(int)c] = i; }
        }

        /// <summary>
        /// Encodes a string of binary data using the Safe32 alphabet with no padding.
        /// </summary>
        /// <param name="data">The binary data to encode. If null or zero-length, an empty string is returned.</param>
        /// <returns>The base32 encoded string</returns>
        public static string Encode(byte[] data)
        {
            if (data == null || data.Length == 0) return "";
            var sb = new StringBuilder(8 * ((data.Length / 5) + 1));

            int b1, b2, b3, b4, b5;
            char[] chars = new char[8];
            int len = data.Length;
            int charCount = 8;

            for (var i = 0; i < len; i++)
            {
                b1 = data[i];
                if (++i < len) b2 = data[i]; else { b2 = 0; charCount -= 2; }
                if (++i < len) b3 = data[i]; else { b3 = 0; charCount--; }
                if (++i < len) b4 = data[i]; else { b4 = 0; charCount -= 2; }
                if (++i < len) b5 = data[i]; else { b5 = 0; charCount--; }

                //char 00000111 11222223 33334444 45555566 66677777 0-based
                //byte 11111111 22222222 33333333 44444444 55555555
                //char 11111222 22333334 44445555 56666677 77788888 1-based

                chars[0] = s_base32[(b1 & 0xf8) >> 3];
                chars[1] = s_base32[((b1 & 0x07) << 2) | ((b2 & 0xc0) >> 6)];
                chars[2] = s_base32[(b2 & 0x3e) >> 1];
                chars[3] = s_base32[((b2 & 0x01) << 4) | ((b3 & 0xf0) >> 4)];
                chars[4] = s_base32[((b3 & 0x0f) << 1) | ((b4 & 0x80) >> 7)];
                chars[5] = s_base32[(b4 & 0x7c) >> 2];
                chars[6] = s_base32[((b4 & 0x03) << 3) | ((b5 & 0xe0) >> 5)];
                chars[7] = s_base32[b5 & 0x1f];

                sb.Append(chars, 0, charCount);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes Safe32 encoded strings. Tolerant of whitespace, line breaks.
        /// </summary>
        /// <param name="encodedStr">The encoded string. If null or zero-length, a zero-length byte array is returned.</param>
        /// <param name="nullOnError">If true, returns a null result instead of throwing a FormatException. Defaults to false.</param>
        /// <returns>The decoded binary data.</returns>
        /// <exception cref="FormatException">Thrown if invalid data is encountered in the input string.</exception>
        public static byte[] Decode(string encodedStr, bool nullOnError = false)
        {
            if (encodedStr == null || encodedStr.Length == 0) return new byte[0];

            var len = 0;
            for (var i = 0; i < encodedStr.Length; i++)
            {
                var ch = encodedStr[i];
                if (char.IsWhiteSpace(ch) || ch == 10 || ch == 13) continue;
                if (ch > 255) { if (nullOnError) return null; else throw new FormatException("Invalid data encountered in string"); }
                len++;
            }

            var remainder = len % 8;
            var byteLen = 5 * (len / 8);
            if (remainder == 1 || remainder == 2) byteLen += 1;
            else if (remainder == 3 || remainder == 4) byteLen += 2;
            else if (remainder == 5) byteLen += 3;
            else if (remainder == 6 || remainder == 7) byteLen += 4;
            var bytes = new byte[byteLen];

            int strIdx = 0;
            int b1, b2, b3, b4, b5;
            int c1, c2, c3, c4, c5, c6, c7, c8;
            bool b2On = true, b4On = true;
            var byteCount = 5;
            int byteIdx = 0;
            var charCount = 0;

            for (var i = 0; i < len; i++)
            {
                c1 = c2 = c3 = c4 = c5 = c6 = c7 = c8 = -1;
                for (charCount = 0; strIdx < encodedStr.Length && charCount < 8; strIdx++)
                {
                    var ch = encodedStr[strIdx];
                    if (char.IsWhiteSpace(ch) || ch == 10 || ch == 13 || ch == '=') continue;

                    charCount++;
                    if (charCount == 1) c1 = s_base32dec[ch];
                    else if (charCount == 2) c2 = s_base32dec[ch];
                    else if (charCount == 3) c3 = s_base32dec[ch];
                    else if (charCount == 4) c4 = s_base32dec[ch];
                    else if (charCount == 5) c5 = s_base32dec[ch];
                    else if (charCount == 6) c6 = s_base32dec[ch];
                    else if (charCount == 7) c7 = s_base32dec[ch];
                    else if (charCount == 8) c8 = s_base32dec[ch];
                }

                if (++i >= len) { c2 = -1; } //required for byte 1
                if (++i >= len) { c3 = 0; byteCount--; b2On = false; } //indicates byte 2
                if (++i >= len) { c4 = b2On ? -1 : 0; } //required for byte 2 if c3 is set
                if (++i >= len) { c5 = 0; byteCount--; } //indicates byte 3
                if (++i >= len) { c6 = 0; byteCount--; b4On = false; } //indicates byte 4
                if (++i >= len) { c7 = b4On ? -1 : 0; } //required for byte 4 if c6 is set
                if (++i >= len) { c8 = 0; byteCount--; } //indicates byte 5

                if (byteCount < 1 || c1 == -1 || c2 == -1 || c3 == -1 || c4 == -1 || c5 == -1 || c6 == -1 || c7 == -1 || c8 == -1)
                { if (nullOnError) return null; else throw new FormatException("Invalid data encountered in string"); }

                b1 = (c1 << 3) | ((c2 & 0x1c) >> 2);
                b2 = ((c2 & 0x03) << 6) | (c3 << 1) | ((c4 & 0x10) >> 4);
                b3 = ((c4 & 0x0f) << 4) | ((c5 & 0x1e) >> 1);
                b4 = ((c5 & 0x01) << 7) | (c6 << 2) | ((c7 & 0x18) >> 3);
                b5 = ((c7 & 0x07) << 5) | c8;

                bytes[byteIdx++] = (byte)b1;
                if (byteCount > 1) bytes[byteIdx++] = (byte)b2;
                if (byteCount > 2) bytes[byteIdx++] = (byte)b3;
                if (byteCount > 3) bytes[byteIdx++] = (byte)b4;
                if (byteCount == 5) bytes[byteIdx++] = (byte)b5;
            }

            return bytes;
        }
    }
}
