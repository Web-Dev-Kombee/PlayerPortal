using System.Text;

namespace Shard.Commons
{
    /// <summary>
    /// SimpleTemplate provides a string format alternative, using named tags instead of numeric placeholders. Tags are then resolved by either a dictionary of arguments or a callback function.
    /// Tags should be compared case-insensitively.
    /// Additionally, a template can be processed once, then re-used to generate additional strings.
    /// The same format syntax is supported, with one addition: {tag[,[-]minwidth][;maxwidth][:formatString]}. Tag may use any characters, including whitespace, except for line breaks, comma, colon, or semicolon (brace must be escaped). Format arguments may contain linebreaks where the property can support it.
    /// ,[-]minwidth operates as per alignment in string.format and pads text to a minimum width. A positive value will add padding to the left, a negative value will add padding to the right.
    /// ;maxwidth specified the maximum width a field can take. Text longer than maxwidth will be truncated. Positive values truncate the end of the string. Negative values truncate the start of the string.
    /// Use double {{ and }} to escape a brace for display.
    /// </summary>
    public class SimpleStringTemplate : IStringTemplate
    {
        public delegate object ArgumentResolver(string key, string format);

        class Particle
        {
            public bool IsLiteral;
            public string Value; //Literal=text; Not literal=tag
            public int Minwidth;
            public int? Maxwidth;
            public string Format;
        }

        class StateData
        {
            public char[] Input;
            public int Index = 0;
            public List<Particle> Particles = new List<Particle>();
            public Particle Working;
            public StringBuilder Buffer = new StringBuilder();

            public StateData(string str)
            {
                Input = str.ToCharArray();
            }
        }

        #region Parser internal structures
        class ParseTransition
        {
            public char Char;
            public Func<char, StateData, bool> CharTest;
            public States NewState;
            public bool AddCharToBuffer;
            public bool ConsumeChar;
            public Action<StateData> Action;
        }
        class ParserState
        {
            public States State;
            public List<ParseTransition> Transitions = new List<ParseTransition>();
            public ParseTransition ElseTransition;
            public ParseTransition TerminalTransition;

            public ParserState(States state)
            {
                State = state;
            }

            public ParserState On(char ch, States newState, bool add = false, bool consumeChar = true, Action<StateData> action = null)
            {
                Transitions.Add(new ParseTransition { Char = ch, NewState = newState, AddCharToBuffer = add, ConsumeChar = consumeChar, Action = action });
                return this;
            }

            public ParserState On(Func<char, StateData, bool> test, States newState, bool add = false, bool consumeChar = true, Action<StateData> action = null)
            {
                Transitions.Add(new ParseTransition { CharTest = test, NewState = newState, AddCharToBuffer = add, ConsumeChar = consumeChar, Action = action });
                return this;
            }

            public ParserState Else(States newState, bool add = false, bool consumeChar = true, Action<StateData> action = null)
            {
                ElseTransition = new ParseTransition { NewState = newState, AddCharToBuffer = add, ConsumeChar = consumeChar, Action = action };
                return this;
            }

            public ParserState Terminal(Action<StateData> action = null)
            {
                TerminalTransition = new ParseTransition { Action = action };
                return this;
            }
        }
        #endregion

        #region Parser states
        public enum States
        {
            Literal, LiteralOpenBrace, LiteralCloseBrace,
            Tag, MinWidth, MaxWidth, Format,
            TagOpen, TagClose, MinWidthClose, MaxWidthClose, FormatOpen, FormatClose
        }

        static ParserState[] s_parser = new ParserState[]{

			//Standard literal text
			new ParserState(States.Literal)
                .On('{', States.LiteralOpenBrace)
                .On('}', States.LiteralCloseBrace)
                .Else(States.Literal, add:true)
                .Terminal(s=>s.Particles.Add(new Particle{ IsLiteral=true, Value=s.Buffer.ToString()})),

			//Open brace encountered in literal text
			new ParserState(States.LiteralOpenBrace)
                .On('{', States.Literal, add:true)
                .Else(States.Tag, consumeChar:false, action:s=>{
                    s.Particles.Add(new Particle{ IsLiteral=true, Value=s.Buffer.ToString()});
                    s.Buffer.Clear();
                    s.Working = new Particle();
                })
                .Terminal(s=>{throw new FormatException("Unescaped { encountered at end of input");}),

			//Close brace encountered in literal text
			new ParserState(States.LiteralCloseBrace)
                .On('}', States.Literal, add:true)
                .Else(States.LiteralCloseBrace, action:s=>{throw new FormatException("Unescaped } encountered outside of tag at index "+s.Index);}),

			//Tag
			new ParserState(States.Tag)
                .On('\n', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in tag at index "+s.Index);})
                .On('\r', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in tag at index "+s.Index);})
                .On(',', States.MinWidth, action:s=>{s.Working.Value=s.Buffer.ToString(); s.Buffer.Clear();})
                .On(';', States.MaxWidth, action:s=>{s.Working.Value=s.Buffer.ToString(); s.Buffer.Clear();})
                .On(':', States.Format, action:s=>{s.Working.Value=s.Buffer.ToString(); s.Buffer.Clear();})
                .On('{', States.TagOpen)
                .On('}', States.TagClose)
                .Else(States.Tag, add:true)
                .Terminal(s=>{throw new FormatException("Unclosed tag at end of input");}),

			//Open brace encountered inside tag
			new ParserState(States.TagOpen)
                .On('{', States.Tag, add:true)//Literal brace
				.Else(States.TagOpen, action:s=>{throw new FormatException("Unescaped { encountered inside of tag at index "+s.Index);}),

			//Close brace encountered in tag
			new ParserState(States.TagClose)
                .On('}', States.Tag, add:true)//Literal brace
				.Else(States.Literal, consumeChar:false, action:s=>{ s.Working.Value = s.Buffer.ToString(); s.Buffer.Clear(); s.Particles.Add(s.Working);}),

			//Min Width
			new ParserState(States.MinWidth)
                .On('\n', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in minWidth at index "+s.Index);})
                .On('\r', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in minWidth at index "+s.Index);})
                .On(';', States.MaxWidth, action:s=>{s.Working.Minwidth=int.Parse(s.Buffer.ToString()); s.Buffer.Clear();})
                .On(':', States.Format, action:s=>{s.Working.Minwidth=int.Parse(s.Buffer.ToString()); s.Buffer.Clear();})
                .On('}', States.MinWidthClose)
                .On((ch,s)=>char.IsDigit(ch)||(ch=='-'&&s.Buffer.Length==0), States.MinWidth, add:true)
                .Else(States.MinWidth, action:s=>{throw new FormatException("Invalid character in minWidth at index "+s.Index);})
                .Terminal(s=>{throw new FormatException("Unclosed tag at end of input");}),

			//Close brace encountered in min width
			new ParserState(States.MinWidthClose)
                .On('}', States.MinWidthClose, action:s=>{throw new FormatException("Invalid character in minWidth at index "+s.Index);}) //literal brace is invalid here
				.Else(States.Literal, consumeChar:false, action:s=>{ s.Working.Minwidth = int.Parse(s.Buffer.ToString()); s.Buffer.Clear(); s.Particles.Add(s.Working);}),

			//Max Width
			new ParserState(States.MaxWidth)
                .On('\n', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in maxWidth at index "+s.Index);})
                .On('\r', States.Tag, action:s=>{throw new FormatException("Linebreak encountered in maxWidth at index "+s.Index);})
                .On(':', States.Format, action:s=>{s.Working.Maxwidth=int.Parse(s.Buffer.ToString()); s.Buffer.Clear();})
                .On('}', States.MaxWidthClose)
                .On((ch,s)=>char.IsDigit(ch)||(ch=='-'&&s.Buffer.Length==0), States.MaxWidth, add:true)
                .Else(States.MaxWidth, action:s=>{throw new FormatException("Invalid character in maxWidth at index "+s.Index);})
                .Terminal(s=>{throw new FormatException("Unclosed tag at end of input");}),

			//Close brace encountered in min width
			new ParserState(States.MaxWidthClose)
                .On('}', States.MaxWidthClose, action:s=>{throw new FormatException("Invalid character in maxWidth at index "+s.Index);}) //literal brace is invalid here
				.Else(States.Literal, consumeChar:false, action:s=>{ s.Working.Maxwidth = int.Parse(s.Buffer.ToString()); s.Buffer.Clear(); s.Particles.Add(s.Working);}),

			//Format
			new ParserState(States.Format)
                .On('{', States.FormatOpen)
                .On('}', States.FormatClose)
                .Else(States.Format, add:true)
                .Terminal(s=>{throw new FormatException("Unclosed tag at end of input");}),

			//Open brace encountered inside format
			new ParserState(States.FormatOpen)
                .On('{', States.Format, add:true)//Literal brace
				.Else(States.FormatOpen, action:s=>{throw new FormatException("Unescaped { encountered inside of format at index "+s.Index);}),

			//Close brace encountered in format
			new ParserState(States.FormatClose)
                .On('}', States.Format, add:true)//Literal brace
				.Else(States.Literal, consumeChar:false, action:s=>{ s.Working.Format = s.Buffer.ToString(); s.Buffer.Clear(); s.Particles.Add(s.Working);}),

        }.OrderBy(v => v.State).ToArray();
        #endregion

        List<Particle> m_particles;

        /// <summary>
        /// Creates a new template based on the provided string.
        /// </summary>
        /// <param name="template">The template string to parse.</param>
        /// <exception cref="FormatException">Throws a format exception if the string could not be parsed.</exception>
        public SimpleStringTemplate(string template)
        {
            m_particles = Parse(template);
        }

        #region ParserState string
        List<Particle> Parse(string template)
        {
            var state = new StateData(template);
            var parser = s_parser[(int)States.Literal];
            int len = state.Input.Length;
            int i = 0, oldi;
            ParseTransition t = null;
            bool isValid;
            char c;
            while (state.Index < len)
            {
                isValid = false;
                c = state.Input[state.Index];

                //Look for transition
                for (i = 0; i < parser.Transitions.Count; i++)
                {
                    t = parser.Transitions[i];
                    if (t.CharTest != null) isValid = t.CharTest(c, state);
                    else isValid = t.Char == c;
                    if (isValid) break;
                }

                if (!isValid && parser.ElseTransition != null)
                {
                    t = parser.ElseTransition;
                    isValid = true;
                }

                if (!isValid)
                    throw new FormatException("Unable to parse input at index " + state.Index);

                //Process transition
                oldi = state.Index;
                if (t.AddCharToBuffer) state.Buffer.Append(c);
                if (t.ConsumeChar) state.Index++;
                if (t.Action != null) t.Action(state);
                if (t.NewState == parser.State && oldi == state.Index)
                    throw new FormatException("Error in parser, failed to advance at index " + state.Index); //Parser failed to switch to a new state OR advance the pointer. Likely stuck.
                parser = s_parser[(int)t.NewState];
            }
            //Check for terminal transition
            t = parser.TerminalTransition ?? parser.ElseTransition;
            if (t != null && t.Action != null)
                t.Action(state);
            //Return particles
            return state.Particles;
        }
        #endregion

        /// <summary>
        /// Formats the template, outputting a string. Arguments are populated using the specified factory.
        /// </summary>
        /// <param name="argFactory">The argument factory to resolve requested parameters</param>
        /// <param name="provider">The format provider to use when formatting values, or null to use the default</param>
        /// <returns>The formatted string</returns>
        public string Format(ArgumentResolver argFactory, IFormatProvider provider = null)
        {
            if (argFactory == null) throw new ArgumentNullException("argFactory", "An argument factory must be provided to format the string");

            ICustomFormatter custFormat = null;
            if (provider != null && provider is ICustomFormatter)
                custFormat = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));

            StringBuilder sb = new StringBuilder();
            Particle p;
            object argVal;
            string formattedString;
            IFormattable formattable;
            int pad;
            bool padLeft;
            int cut;
            bool cutLeft = false;

            for (var i = 0; i < m_particles.Count; i++)
            {
                p = m_particles[i];
                if (p.IsLiteral)
                    sb.Append(p.Value);
                else
                {
                    argVal = argFactory(p.Value, p.Format);

                    formattedString = null;
                    if (custFormat != null)
                        formattedString = custFormat.Format(p.Format, argVal, provider);

                    if (formattedString == null)
                    {
                        formattable = argVal as IFormattable;
                        if (formattable != null)
                            formattedString = formattable.ToString(p.Format, provider);
                        else if (argVal != null)
                            formattedString = argVal.ToString();
                    }

                    if (formattedString == null)
                        formattedString = String.Empty;

                    //Get padding width; will be positive if pad needed; -ve if not.
                    padLeft = p.Minwidth > 0;
                    if (padLeft) pad = p.Minwidth - formattedString.Length;
                    else pad = -p.Minwidth - formattedString.Length;

                    //Get amount to cut; will be positive if cut needed; -ve if not.
                    if (p.Maxwidth.HasValue)
                    {
                        cutLeft = p.Maxwidth < 0;
                        if (pad < 0) pad = 0;
                        if (cutLeft) cut = (formattedString.Length + pad) + p.Maxwidth.Value;
                        else cut = (formattedString.Length + pad) - p.Maxwidth.Value;
                    }
                    else
                        cut = 0;

                    if (cut > 0 && pad > 0)
                    {
                        if (cut > pad) { cut -= pad; pad = 0; } //Need to cut more than we pad
                        else { pad -= cut; cut = 0; } //Need to pad more than we cut
                    }

                    if (padLeft && pad > 0) sb.Append(' ', pad);
                    if (cut <= 0) sb.Append(formattedString);
                    else if (cutLeft) sb.Append(formattedString, cut, formattedString.Length - cut);
                    else sb.Append(formattedString, 0, formattedString.Length - cut);
                    if (!padLeft && pad > 0) sb.Append(' ', pad);
                }
            }

            return sb.ToString();
        }
    }
}
