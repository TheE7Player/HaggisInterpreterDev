using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Haggis_Interpreter
{
    /// <summary>
    /// Syntax Highlighting Support
    /// </summary>
    class HaggisLexer
    {
        public static HybridDictionary identifiers = new HybridDictionary();
        // Build-up from wiki from repo: https://github.com/jacobslusser/ScintillaNET/wiki/Custom-Syntax-Highlighting

        public const int StyleDefault = 0;
        public const int StyleKeyword = 1;
        public const int StyleIdentifier = 2;
        public const int StyleNumber = 3;
        public const int StyleString = 4;
        public const int StyleComment = 5;

        private const int STATE_UNKNOWN = 0;
        private const int STATE_IDENTIFIER = 1;
        private const int STATE_NUMBER = 2;
        private const int STATE_STRING = 3;
        private const int STATE_COMMENT = 5;

        private HashSet<string> keywords;

        private string currentIdentifier = string.Empty;

        public void Style(ScintillaNET.Scintilla scintilla, int startPos, int endPos)
        {
            // Back up to the line start
            int line = scintilla.LineFromPosition(startPos);
            startPos = scintilla.Lines[line].Position;
            var length = 0;
            var state = STATE_UNKNOWN;          
            Int32 style = 0;
            char c = '\0';

            // Start styling
            scintilla.StartStyling(startPos);
            while (startPos < endPos)
            {
                c = (char)scintilla.GetCharAt(startPos);

            REPROCESS:
                switch (state)
                {
                    case STATE_UNKNOWN:
                        if (c == '"')
                        {
                            // Start of "string"
                            scintilla.SetStyling(1, StyleString);
                            state = STATE_STRING;
                        }
                        else if (Char.IsDigit(c))
                        {
                            state = STATE_NUMBER;
                            goto REPROCESS;
                        }
                        else if (Char.IsLetter(c))
                        {
                            state = STATE_IDENTIFIER;
                            goto REPROCESS;
                        }
                        else if(c == '#')
                        {
                            state = STATE_COMMENT;
                            goto REPROCESS;
                        }
                        else
                        {
                            // Everything else
                            scintilla.SetStyling(1, StyleDefault);
                        }
                        break;

                    case STATE_STRING:
                        if (c == '"')
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
                        if (Char.IsLetterOrDigit(c))
                        {
                            length++;
                        }
                        else
                        {
                            style = StyleDefault;
                            currentIdentifier = scintilla.GetTextRange(startPos - length, length);

                            if (keywords.Contains(currentIdentifier))
                            { style = StyleKeyword; }
                            else
                            {
                                if (!(identifiers is null))
                                {
                                    bool exists =  identifiers.Values.OfType<string>().Any(z => z.Contains(currentIdentifier));
      
                                    if (exists)
                                    {
                                        style = StyleIdentifier;
                                    }
                                }
                            }

                            scintilla.SetStyling(length, style);
                            length = 0;
                            state = STATE_UNKNOWN;
                            goto REPROCESS;
                        }
                        break;

                    case STATE_COMMENT:
                        if (c != '\n')
                        { 
                            length++;
                            state = STATE_COMMENT;

                            try
                            {
                                scintilla.SetStyling(1, STATE_COMMENT);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Problem");
                                //scintilla.SetStyling(endPos - startPos, STATE_COMMENT);
                            }

                        }
                        else
                        {
                            if (length == startPos)
                                scintilla.SetStyling(1, STATE_COMMENT);
                            else
                            {
                                try
                                {
                                    scintilla.SetStyling(startPos - length, STATE_COMMENT);
                                }
                                catch (Exception)
                                {
                                    if(!scintilla.Lines[line].Text.Contains("\r") || !scintilla.Lines[line].Text.Contains("\n"))
                                        scintilla.SetStyling(scintilla.Lines[line].EndPosition - scintilla.Lines[line].Position, STATE_COMMENT);
                                }
                                
                            }
                        }
                        break;
                }

                startPos++;
            }
        }

        public HaggisLexer(string keywords)
        {
            // Put keywords in a HashSet
            var list = System.Text.RegularExpressions.Regex.Split(keywords ?? string.Empty, @"\s+").Where(l => !string.IsNullOrEmpty(l));
            this.keywords = new HashSet<string>(list);
        }
    }
}
