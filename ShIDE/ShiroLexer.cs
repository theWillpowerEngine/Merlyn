using ScintillaNET;
using Shiro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShIDE
{
    public class ShiroLexer
    {
        public const int StyleDefault = 0;
        public const int StyleKeyword = 1;
        public const int StyleComment = 2;
        public const int StyleString = 3;
        public const int StyleNumber = 4;
        public const int StyleFunction = 5;
        public const int StyleVariable = 6;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_IDENTIFIER = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_STRING = 3;
        private const int STATE_COMMENT = 4;
        
        private HashSet<string> keywords;
        public static Interpreter Shiro;

        public void Style(Scintilla scintilla, int startPos, int endPos)
        {
            // Back up to the line start
            var line = scintilla.LineFromPosition(startPos);
            startPos = scintilla.Lines[line].Position;

            var length = 0;
            var state = STATE_UNKNOWN;

            // Start styling
            scintilla.StartStyling(startPos);
            char stringDelim = '~';
            while (startPos < endPos)
            {
                var c = (char)scintilla.GetCharAt(startPos);

                REPROCESS:
                switch (state)
                {
                    case STATE_UNKNOWN:
                        if (c == '"' || c == '`')
                        {
                            // Start of "string"
                            stringDelim = c;
                            scintilla.SetStyling(1, StyleString);
                            state = STATE_STRING;
                        }
                        else if(c == '\'' && (startPos+1 <= endPos && (char)scintilla.GetCharAt(startPos+1) != '('))
                        {
                            // Start of "string"
                            stringDelim = c;
                            scintilla.SetStyling(1, StyleString);
                            state = STATE_STRING;
                        }
                        else if (c == ';')
                        {
                            state = STATE_COMMENT;
                        }
                        else if (Char.IsDigit(c))
                        {
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        }
                        else if (Char.IsLetter(c) || c == '.' || c == '?' || c == '+' || c == '-' || c == '<' || c == '!' || c == '=' || c == '*' || c == '/')
                        {
                            state = STATE_IDENTIFIER;
                            goto REPROCESS;
                        }
                        else
                        {
                            // Everything else
                            scintilla.SetStyling(1, StyleDefault);
                        }
                        break;

                    case STATE_STRING:
                        if (c == stringDelim)
                        {
                            length++;
                            scintilla.SetStyling(length, StyleString);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            length++;
                        }
                        break;

                    case STATE_COMMENT:
                        if (c == '\r' || c == '\n')
                        {
                            length++;
                            scintilla.SetStyling(length, StyleComment);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        } 
                        else if (c == ';')
                        {
                            length += 2;
                            scintilla.SetStyling(length, StyleComment);
                            length = 0;
                            state = STATE_UNKNOWN;
                        }
                        else
                        {
                            length++;
                        }
                        break;

                    case STATE_NUMBER:
                        if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == 'x')
                        {
                            length++;
                        }
                        else
                        {
                            scintilla.SetStyling(length, StyleNumber);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_IDENTIFIER:
                        if (Char.IsLetterOrDigit(c) || c == '.' || c == '?' || c == '+' || c == '-' || c == '<' || c == '!' || c == '=' || c == '*' || c == '/')
                        {
                            length++;
                        }
                        else
                        {
                            var style = StyleDefault;
                            var identifier = scintilla.GetTextRange(startPos - length, length);
                            if (keywords.Contains(identifier))
                                style = StyleKeyword;
                            if (Shiro.IsFunctionName(identifier))
                                style = StyleFunction;
                            if (Shiro.IsVariableName(identifier.TrimStart('$')))
                                style = StyleVariable;

                            scintilla.SetStyling(length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;
                }

                startPos++;
            }
        }

        public ShiroLexer(string keywords)
        {
            keywordString = keywords;
            var list = Regex.Split(keywords ?? string.Empty, @"\s+").Where(l => !string.IsNullOrEmpty(l));
            this.keywords = new HashSet<string>(list);
        }

        private static string keywordString;
        internal static string GetAutoCompleteItems()
        {
            return keywordString;
        }
    }
}
