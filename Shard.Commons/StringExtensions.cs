using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Shard.Commons
{
    /// <summary>
    /// Provides string-related convenience extensions
    /// </summary>
    public static class StringExtensions
    {
        static Regex rxTrimWS = new Regex(@"\A\s*(.*?)\s*\Z", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Test if the string is null or blank (consisting of only whitespace)
        /// </summary>
        /// <param name="input">The input string to test</param>
        /// <returns>True if null or blank, false if not</returns>
        public static bool IsNullOrBlank(this string input) { return string.IsNullOrWhiteSpace(input); }

        /// <summary>
        /// Test if the string is null or blank (consisting of only whitespace) and uses a substitute value if so
        /// </summary>
        /// <param name="input">The input string to test</param>
        /// <param name="value">The replacement value to use if the string was null or blank</param>
        /// <returns>The replacement value if null or blank, or the original string if not</returns>
        public static string IfNullOrBlank(this string input, string value) { return string.IsNullOrWhiteSpace(input) ? value : input; }

        /// <summary>
        /// Test if the string is null or blank (consisting of only whitespace) and executes a function if so
        /// </summary>
        /// <param name="input">The input string to test</param>
        /// <param name="func">The function to invoke and return the result of if the string was blank/null</param>
        /// <returns>The result if the invoked function if null or blank, or the original string if not</returns>
        public static string IfNullOrBlank(this string input, Func<string, string> func) { return string.IsNullOrWhiteSpace(input) ? func(input) : input; }

        /// <summary>
        /// Test if the string is null or blank (consisting of only whitespace) and executes a function if not
        /// </summary>
        /// <param name="input">The input string to test</param>
        /// <param name="func">The function to invoke and return the result of if the string was not blank/null</param>
        /// <returns>The original null/blank string, or the result if the invoked function if not</returns>
        public static string IfNotNullOrBlank(this string input, Func<string, string> func) { return string.IsNullOrWhiteSpace(input) ? input : func(input); }

        /// <summary>
        /// Performs an invariant-culture case-ignorant test to see if a string equals the nominated text.
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <param name="comparand">The value to compare the input string to</param>
        /// <returns>True if a match, false if not</returns>
        public static bool Is(this string input, string comparand) { return string.Compare(input, comparand, StringComparison.OrdinalIgnoreCase) == 0; }

        /// <summary>
        /// Performs an invariant-cultre case-ignorant test to see if a string contains the nominated text.
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <param name="value">The value that the input string should contain</param>
        /// <returns>True if successful, false if not or either string is null/empty</returns>
        public static bool Has(this string str, string value)
        {
            if (str == null || str == "" || value == null || value == "") return false;
            return str.ToLowerInvariant().Contains(value.ToLowerInvariant());
        }

        /// <summary>
        /// Performs an invariant-cultre case-ignorant test to see if a string starts with the nominated text.
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <param name="value">The value that the input string should begin with</param>
        /// <returns>True if successful, false if not or either string is null/empty</returns>
        public static bool StartsAs(this string str, string value)
        {
            if (str == null || str == "" || value == null || value == "") return false;
            return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Performs an invariant-cultre case-ignorant test to see if a string ends with the nominated text.
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <param name="value">The value that the input string should end with</param>
        /// <returns>True if successful, false if not or either string is null/empty</returns>
        public static bool EndsAs(this string str, string value)
        {
            if (str == null || str == "" || value == null || value == "") return false;
            return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Removes whitespace from the ends of a string (any character matching the Regex whitespace definition)
        /// </summary>
        /// <param name="inputText">The string to trim</param>
        /// <returns>The trimmed text, or null if input is null</returns>
        public static string TrimWhitespace(this string inputText)
        {
            if (inputText == null) return null;
            try
            {
                return rxTrimWS.Replace(inputText, "$1");
            }
            catch (OutOfMemoryException)
            {
                // Fallback to string.Trim() in case of OutOfMemoryException
                return inputText.Trim();
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle regex timeout exceptions if they occur
                return inputText.Trim();
            }
            catch (Exception e)
            {
                try
                {
                    // if not string any other characters then concat to string to become string
                    return (inputText + "").Trim();
                }
                catch (Exception e2)
                {
                    e2.Data.Add("Input String", inputText);
                    e2.Data.Add("Origianal Exception", e.Message);
                    e2.Data.Add("Origianal Stack Trace", e.StackTrace);
                    e2.Data.Add("Origianal Source", e.Source);
                    throw e2;
                }
            }
        }

        public static string TrimWhiteSpaceCGPT(this string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return inputText;

            int startIndex = 0;
            int endIndex = inputText.Length - 1;

            // Find the start index of non-whitespace characters
            while (startIndex < inputText.Length && char.IsWhiteSpace(inputText[startIndex]))
            {
                startIndex++;
            }

            // If the entire string is whitespace
            if (startIndex == inputText.Length)
                return string.Empty;

            // Find the end index of non-whitespace characters
            while (endIndex >= 0 && char.IsWhiteSpace(inputText[endIndex]))
            {
                endIndex--;
            }

            // Calculate the length of the trimmed string
            int length = endIndex - startIndex + 1;

            // Extract the trimmed string
            return length > 0 ? inputText.Substring(startIndex, length) : string.Empty;
        }

        /// <summary>
        /// Decodes a string of URL encoded text. Plus characters are decoded to spaces.
        /// </summary>
        /// <param name="str">The string of encoded text. This should not be called on text that is not already encoded.</param>
        /// <returns>The encoded text, or null if input is null</returns>
        public static string UrlDecode(this string str) { return str == null ? null : Uri.UnescapeDataString(str.Replace("+", " ")); }

        /// <summary>
        /// Encodes a string of text for safe use within a URL. Escapes all chacters except for the RFC 2396 unreserved characters. Assumes no escape sequences are present.
        /// </summary>
        /// <param name="str">The string of text to encode. Must be less than 32766 characters or a UriFormatException will be thrown.</param>
        /// <returns>The encoded text, or null if input is null</returns>
        public static string UrlEncode(this string str) { return str == null ? null : Uri.EscapeDataString(str); }

        /// <summary>
        /// Encodes a string of text for safe display in an HTML display.
        /// </summary>
        /// <param name="str">The text to encode</param>
        /// <returns>The encoded text, or null if input is null</returns>
        public static string HtmlEncode(this string str) { return str == null ? null : WebUtility.HtmlEncode(str); }

        /// <summary>
        /// Decodes a string of html into plain text
        /// </summary>
        /// <param name="str">The html encoded text</param>
        /// <returns>The plain text after decoding</returns>
        public static string HtmlDecode(this string str) { return str == null ? null : WebUtility.HtmlDecode(str); }


        /// <summary>
        /// Encodes a string of text suitable for use in an html attribute. Minimally encodes - quote, ampersand and opening brackets only are encoded.
        /// </summary>
        /// <param name="str">The text to encode</param>
        /// <returns>The encoded text, or null if input is null</returns>
        public static string HtmlAttributeEncode(this string str)
        {
            if (str == null) return null;
            var sb = new StringBuilder(str.Length);
            var len = str.Length;
            for (var i = 0; i < len; i++)
            {
                switch (str[i])
                {
                    case '"': sb.Append("&quot;"); break;
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    default: sb.Append(str[i]); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Removes or replaces characters that are not 'uri segment human friendly'. Operates with a very restricted character set.
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>A url path segment friendly string</returns>
        public static string MakeUrlFriendly(this string str)
        {
            if (str == null) return "";
            var url = new StringBuilder();
            var hyphenation = 0; //0=Normal. 1=We've already added an explicit seperator. 2=Add a hyphen
            foreach (char ch in str)
            {
                if ((ch >= 'A' && ch <= 'Z') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') ||
                    ch == '&' || ch == '+')
                {
                    //It is an allowable character.

                    //Add hyphen first?
                    if (hyphenation == 2) url.Append('-');

                    if (ch == '&') { url.Append("and"); hyphenation = 2; }
                    else if (ch == '+') { url.Append("plus"); hyphenation = 2; }
                    else { url.Append(ch); hyphenation = 0; }
                }
                else if (ch == '_' || ch == '-')
                {
                    //Underscores and hyphens override auto-hyphenation
                    hyphenation = 1; //Hyphens will be ignored until a regular character is added again
                    url.Append(ch);
                }
                else if (ch == '\'') { } //We completely ignore ' as if they never existed
                else
                {
                    //Bad character; if we've not explicitly added a seperator, set to add a hyphen on the next regular character.
                    if (hyphenation != 1) hyphenation = 2;
                }
            }
            return url.ToString();
        }

        /// <summary>
        /// Fixes line break sequences (\r, \n, \r\n) in a string of input text
        /// </summary>
        /// <param name="inputText">The text to normalise</param>
        /// <param name="replaceWith">The sequence to replace line breaks with. Defaults to \r\n</param>
        /// <returns>The normalised text, or null if input is null.</returns>
        public static string FixLineBreaks(this string inputText, string replaceWith = "\r\n")
        {
            if (inputText == null) return null;
            return (inputText ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", replaceWith);
        }

        //Semi-intelligently converts HTML into plain text
        static Regex ps = new Regex(@"<\s*/p\s*>", RegexOptions.IgnoreCase);
        static Regex brs = new Regex(@"<\s*br\s*/?\s*>", RegexOptions.IgnoreCase);
        static Regex tags = new Regex("<(.|\\n)+?>", RegexOptions.IgnoreCase);
        static Regex doubspace = new Regex(@"[\s-[\r\n]]{2,}", RegexOptions.IgnoreCase);

        /// <summary>
        /// Attempts to apply some basic rules to HTML to produce a readable text version.
        /// </summary>
        /// <param name="inputHTML">The HTML-formatted text to convert. Null input produces null output</param>
        /// <returns>A plain-text representation of the original HTML text, or null if null input provided</returns>
        public static string ToTextFromHtml(this string inputHTML)
        {
            if (inputHTML == null) return null;
            return HtmlDecode(doubspace.Replace(tags.Replace(brs.Replace(ps.Replace(inputHTML, "\r\n\r\n"), "\r\n"), " "), " "));
        }

        static Regex link = new Regex(@"((?:http|ftp|https)://[%\./\-\w]+)", RegexOptions.IgnoreCase);

        /// <summary>
        /// Converts plain text into formatted HTML.
        /// </summary>
        /// <param name="inputText">The plain text to convert. Null input produces null output</param>
        /// <param name="usePTags">Causes the output text to be wrapped in P tags, with paragraph breaks added on double-line breaks</param>
        /// <param name="doLinks"></param>
        /// <returns>An html representation of the text, or null if null input provided</returns>
        public static string ToHtmlFromText(this string inputText, bool usePTags = false, LinkConversion doLinks = LinkConversion.No)
        {
            if (inputText == null) return null;
            var cleaned = FixLineBreaks(inputText, "\n").Split('\n').Select(v => v.TrimWhitespace()).Join("\n");
            var html = WebUtility.HtmlEncode(cleaned).FixLineBreaks("<br/>\r\n");
            if (doLinks == LinkConversion.Normal) html = link.Replace(html, "<a href=\"$1\">$1</a>");
            else if (doLinks == LinkConversion.TargetBlank) html = link.Replace(html, "<a href=\"$1\" target=\"_blank\">$1</a>");
            if (usePTags)
                return "<p>" + html.Replace("<br/>\r\n<br/>\r\n", "</p><p>") + "</p>";
            else
                return html;
        }

        /// <summary>
        /// Trims a string to a given length, optionally adding elipses if the string is too long. Whitespace will be trimmed.
        /// </summary>
        /// <param name="inputText">The string to trim</param>
        /// <param name="maxLen">The maximum length to allow</param>
        /// <param name="addEllipsis">Indicates if it should add an ellipsis (...)</param>
        /// <returns>The trimmed text, or an empty string if null.</returns>
        public static string TrimTo(this string inputText, int maxLen, bool addEllipsis = false)
        {
            return TrimTo(inputText, maxLen, addEllipsis ? "..." : null, false);
        }

        /// <summary>
        /// Trims a string to a given length, optionally adding custom ellipsis if the string is too long, either within the limit or after it. Whitespace will be trimmed.
        /// </summary>
        /// <param name="inputText">The string to trim</param>
        /// <param name="maxLen">The maximum length to allow</param>
        /// <param name="ellipsis">The ellipsis text to add if too long (or null to add nothing)</param>
        /// <param name="ellipsisAppend">True if the ellipsis text should be added after trimming (making the length maxLen+ellipsisLen) or False to fit the ellipsis within the max length</param>
        /// <returns>The trimmed text, or an empty string if null.</returns>
        public static string TrimTo(this string inputText, int maxLen, string ellipsis, bool ellipsisAppend)
        {
            if (inputText == null) return "";
            var txt = inputText.TrimWhitespace();
            if (txt.Length <= maxLen) return txt;
            if (ellipsis == null || ellipsisAppend) return txt.Substring(0, maxLen) + ellipsis;
            if (ellipsis.Length >= maxLen) return ellipsis.Substring(0, maxLen);
            return txt.Substring(0, maxLen - ellipsis.Length) + ellipsis;
        }

        /// <summary>
        /// Fluent-style alias for string.Format()
        /// </summary>
        /// <param name="formatString">The formatter string</param>
        /// <param name="args">The arguments to format</param>
        /// <returns>The formatted string, using the same rules as string.Format()</returns>
        public static string FormatWith(this string formatString, params object[] args) { return string.Format(formatString, args); }

        /// <summary>
        /// Fluent-style alias for string.Format()
        /// </summary>
        /// <param name="args">The arguments to format</param>
        /// <param name="formatString">The formatter string</param>
        /// <returns>The formatted string, using the same rules as string.Format()</returns>
        public static string FormatAs(this object[] args, string formatString) { return string.Format(formatString, args); }

        /// <summary>
        /// Fluent-style alias for a case-insensitive new SimpleStringTemplate().Format() call
        /// </summary>
        /// <param name="obj">An object to format</param>
        /// <param name="formatString">The formatter string</param>
        /// <returns>The formatted string, using the same rules as a case-insensitive .Format()</returns>
        public static string FormatAs(this object obj, string formatString)
        {
            var args = obj.ToDictionary();
            var formatter = new SimpleStringTemplate(formatString);
            return formatter.Format(args, true);
        }

        /// <summary>
        /// Fluent-style alias for a case-insensitive new SimpleStringTemplate().Format() call
        /// </summary>
        /// <param name="args">An argument to format</param>
        /// <param name="formatString">The formatter string</param>
        /// <returns>The formatted string, using the same rules as a case-insensitive .Format()</returns>
        public static string FormatAs(this IDictionary<string, object> args, string formatString)
        {
            var formatter = new SimpleStringTemplate(formatString);
            return formatter.Format(args, true);
        }

        /// <summary>
        /// Formats the template, outputting a string. Arguments are populated from the provided dictionary.
        /// </summary>
        /// <param name="args">The arguments to populate the template from. Note that this should be a case insensitive dictionary.</param>
        /// <param name="makeArgsCaseInsensitive">Ensures that arguments are looked up in a case-insensitive manner (incurs a slight performance/memory cost). Use if the source is uncertain.</param>
        /// <param name="provider">The format provider to use when formatting values, or null to use the default</param>
        /// <returns>The formatted string</returns>
        public static string Format(this IStringTemplate template, IDictionary<string, object> args, bool makeArgsCaseInsensitive = false, IFormatProvider provider = null)
        {
            var useArgs = makeArgsCaseInsensitive
                ? args.ToDictionary(v => v.Key, v => v.Value, StringComparer.OrdinalIgnoreCase)
                : args;
            return template.Format((key, format) => useArgs.GetValue(key), provider);
        }

        /// <summary>
        /// Returns the english ordinal string of a number (1st, 2nd etc)
        /// </summary>
        /// <param name="i">The integer to return an ordinal string for</param>
        /// <param name="formatter">An optional formatter. The number is {0} and the ordinal suffix is {1}</param>
        /// <returns></returns>
        public static string ToOrdinalString(this int i, string formatter = null)
        {
            var suffix = "th";
            var test = i % 100;
            if (test < 10 || test > 19)
            {
                test = test % 10;
                if (test == 1) suffix = "st";
                else if (test == 2) suffix = "nd";
                else if (test == 3) suffix = "rd";
            }

            if (formatter.IsNullOrBlank()) return i + suffix;
            else return formatter.FormatWith(i, suffix);
        }

        /// <summary>
        /// Converts a string to a variable/JS friendly camel case, ie, "ThisTitle", "This Title" and "this title" all become "thisTitle"
        /// Only letters and numbers are retained. All other characters are removed.
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <param name="prefixNumberWithUnderscore">If the first character is a number, then prefix the result with an _ ("12" becomes "_12" - off by default)</param>
        /// <returns>A camel-case version of the string, or an empty string on bad input</returns>
        public static string ToCamelCase(this string value, bool prefixNumberWithUnderscore = false)
        {
            if (value == null) return "";
            //Get word parts
            var parts = new List<string>();
            var sb = new StringBuilder();
            bool? isCaps = null;
            foreach (var ch in value)
            {
                //Bad char = a word break
                if (!char.IsLetterOrDigit(ch))
                {
                    if (sb.Length > 0)
                    {
                        parts.Add(sb.ToString());
                        sb.Clear();
                    }
                    isCaps = null;
                }
                else if (char.IsUpper(ch))
                {
                    if (!isCaps.HasValue || isCaps == true)
                    {
                        isCaps = true;
                        sb.Append(ch);
                    }
                    else
                    {
                        //Capital letter after a lower case letter = a part break
                        if (sb.Length > 0)
                        {
                            parts.Add(sb.ToString());
                            sb.Clear();
                        }
                        isCaps = null;
                        sb.Append(ch);
                    }
                }
                else //lowercase leter/digit/underscore
                {
                    isCaps = false;
                    sb.Append(ch);
                }
            }
            if (sb.Length > 0) parts.Add(sb.ToString());

            //Build result string
            if (parts.Count == 0) return "";
            sb.Clear();
            if (prefixNumberWithUnderscore && char.IsDigit(parts[0][0])) sb.Append("_");
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (i == 0) sb.Append(part.ToLowerInvariant());
                else
                {
                    sb.Append(char.ToUpperInvariant(part[0]));
                    if (part.Length > 1) sb.Append(part.Substring(1).ToLowerInvariant());
                }

            }
            return sb.ToString();
        }
        /// <summary>
        /// Converts a string to a variable/JS friendly camel case, ie, "ThisTitle", "This Title" and "this title" all become "ThisTitle"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string s)
        {
            var x = ToCamelCase(s);
            return char.ToUpper(x[0]) + x.Substring(1);
        }

        /// <summary>
        /// Cleans extended unicode characters from a string, converting some common unicode cases to an ascii representation where possible.
        /// </summary>
        /// <param name="inputText">The text to clean</param>
        /// <returns>A clean string, or null if the input is null</returns>
        public static string ToAscii(this string inputText)
        {
            if (inputText == null) return null;
            StringBuilder output = new StringBuilder();
            foreach (char ch in inputText)
                switch (ch)
                {
                    #region Specific character substitutions
                    case (char)28: output.Append(' '); break;
                    case (char)29: output.Append(' '); break;
                    case (char)160: output.Append(' '); break;
                    case (char)161: output.Append('!'); break;
                    case (char)162: output.Append('c'); break;
                    case (char)163: output.Append('#'); break;
                    case (char)164: output.Append('*'); break;
                    case (char)165: output.Append('Y'); break;
                    case (char)166: output.Append('|'); break;
                    case (char)167: output.Append('S'); break;
                    case (char)168: output.Append('-'); break;
                    case (char)169: output.Append("(c)"); break;
                    case (char)170: output.Append('a'); break;
                    case (char)171: output.Append("<<"); break;
                    case (char)172: output.Append('-'); break;
                    case (char)173: output.Append('-'); break;
                    case (char)174: output.Append("(R)"); break;
                    case (char)175: output.Append('-'); break;
                    case (char)176: output.Append('*'); break;
                    case (char)177: output.Append("+/-"); break;
                    case (char)178: output.Append("sq"); break;
                    case (char)179: output.Append("^3"); break;
                    case (char)180: output.Append('\''); break;
                    case (char)181: output.Append('u'); break;
                    case (char)182: output.Append(""); break;
                    case (char)183: output.Append('*'); break;
                    case (char)184: output.Append(','); break;
                    case (char)185: output.Append("^1"); break;
                    case (char)186: output.Append('*'); break;
                    case (char)187: output.Append(">>"); break;
                    case (char)188: output.Append("1/4"); break;
                    case (char)189: output.Append("1/2"); break;
                    case (char)190: output.Append("3/4"); break;
                    case (char)191: output.Append('?'); break;
                    case (char)231: output.Append('c'); break;
                    case (char)233: output.Append('e'); break;
                    case (char)8202: output.Append('-'); break;
                    case (char)8208: output.Append(""); break;
                    case (char)8210: output.Append('-'); break;
                    case (char)8211: output.Append('-'); break;
                    case (char)8212: output.Append('-'); break;
                    case (char)8213: output.Append('-'); break;
                    case (char)8216: output.Append('\''); break;
                    case (char)8217: output.Append('\''); break;
                    case (char)8220: output.Append('"'); break;
                    case (char)8221: output.Append('"'); break;
                    case (char)8226: output.Append('*'); break;
                    case (char)8230: output.Append("..."); break;
                    case (char)8275: break;
                    case (char)8482: output.Append("(tm)"); break;
                    #endregion
                    default:
                        //Simply remove any other extended characters
                        if ((ch >= '!' && ch <= '~') || ch == ' ' || ch == '\r' || ch == '\n') output.Append(ch);
                        break;
                }
            return output.ToString();
        }

        /// <summary>
        /// Indicates if a string contains any non-ascii friendly characters
        /// </summary>
        /// <param name="inputText">The text to check</param>
        /// <returns>True if ascii-friendly (or null), else false</returns>
        public static bool IsAscii(this string inputText)
        {
            if (inputText == null) return true;
            foreach (var c in inputText) if (c >= 256) return false;
            return true;
        }

        //public static bool IsExpired(this DateTime specificDate)
        //{
        //    return specificDate < DateTime.Now;
        //}

        public static bool IsExpired(this DateTime specificDate, DateTime givenDate)
        {
            return specificDate < givenDate;
        }

        public static string SplitShipToLocFromKey(this string splitShipKey)
        {
            string shipToKey = splitShipKey;
            if (!string.IsNullOrEmpty(splitShipKey) && splitShipKey.IndexOf('-') >= 0)
            {
                shipToKey = shipToKey.Split('-')[0];
            }

            return shipToKey;
        }

    }
}
