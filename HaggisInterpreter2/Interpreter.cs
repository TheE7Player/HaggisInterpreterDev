using System;
using System.Collections.Generic;
using System.Linq;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
        internal static string[] file;

        public static bool executionHandled = false;

        public Dictionary<string, Value> variables { private set; get; }
        public Stack<string> callStack { private set; get; }

        internal static Dictionary<FuncMetaData, int> function { get; private set; }

        public static HSocket server { get; private set; }

        public static int Line;
        public static int Column;

        private void Log(string message) => Console.WriteLine(message);    
        private string Input() => Console.ReadLine();

        private void SendSocketMessage(string title, string information)
        {
            if (server is null)
                return;

            server.QueueMessage(title, information);
        }

        public static int[] errorArea { private set; get; }
        public static string[] errorCaller { private set; get; }
        private List<StatementBlock> CachedIf { get; set; }

        internal struct CommentRange
        {
            public int Start;
            public int End;
            public bool InRange(int currentIndex)
            {
                if (currentIndex >= Start && currentIndex <= End)
                    return true;
                else
                    return false;
            }
        }

        private List<CommentRange> CommentsRanges;
        private readonly string[] validTypes = {"INTEGER", "CHARACTER", "BOOLEAN", "REAL", "STRING"};
        private readonly Dictionary<string, dynamic> DefaultVal = new Dictionary<string, dynamic>{
            {"INTEGER", 0 },
            {"CHARACTER", ' ' },
            {"BOOLEAN", false },
            {"REAL", 0.0 },
            {"STRING", string.Empty }
        };

        #region INTERPRETER_FLAGS
        public struct FLAGS
        {
            /// <summary>
            /// INTERPRETER FLAG: Flag which prints the line in the script when it calls a print function (SEND)
            /// </summary>
            public bool DebugSendRequests { get; set; }
            /// <summary>
            ///  INTERPRETER FLAG: Value of flags to allow auto input if request (RECIEVE)
            /// </summary>
            public Dictionary<string, string> Inputs { get; set; }
        }

        public FLAGS _flags { get; private set; }
        #endregion

        private Tuple<Dictionary<int,string>, Dictionary<int, string>> GenerateStatements(ref int i, string[] _f)
        {
            Dictionary<int, string> t = new Dictionary<int, string>(1);
            Dictionary<int, string> f = new Dictionary<int, string>(1);
            int ifStatementStart = 0;
            int lastIndex = 0;

            while (i < _f.Length)
            {
                if (i >= 0)
                    lastIndex = i;
                else 
                { 
                    Error("PROBLEM PARSING IF STATEMENT - FAULT IN INTERPRETER OR MISBALANCED IF STATEMENT", _f[lastIndex]);
                    break;
                }

                _f[i] = _f[i].Trim();

                if (_f[i].StartsWith("IF")) 
                {
                    ifStatementStart = i + 1;
                    i = AddIfStatement(ref i, _f, true); 
                    t.Add(ifStatementStart, _f[ifStatementStart-1]);
                    continue;
                }

                //(_f[i-1] == "END IF" || string.IsNullOrEmpty(_f[i])
                if (_f[i] == "END IF")
                {
                    break;
                }

                if(_f[i].StartsWith("ELSE IF"))
                {
                    ifStatementStart = i + 1;
                    _f[i] = _f[i].Replace("ELSE", "").Trim();
                    i = AddIfStatement(ref i, _f, true);
                    f.Add(ifStatementStart, _f[ifStatementStart - 1]);
                    continue;
                }

                if (_f[i] != "ELSE")
                {
                    t.Add(i + 1, _f[i]);
                }
                else
                {
                    i++;
                    f.Add(i + 1, _f[i]);
                }

                i++;
            }
            
            return new Tuple<Dictionary<int, string>, Dictionary<int, string>>(t, f);
        }

        private int AddIfStatement(ref int index, string[] Contents, bool closeOnFind = false)
        {
            var sb = new StatementBlock();
            string cond = "";
            char[] trimArray = new char[] { '\r', '\n', '\t', ' ' };

            for (int i = index; i < Contents.Length; i++)
            {
                Contents[i] = Contents[i].Trim(trimArray);
                
                if(Contents[i].StartsWith("###"))
                {
                    for (int z = i + 1; z < Contents.Length; z++)
                    {
                        Contents[z] = Contents[z].Trim(trimArray);
                        if (Contents[z].StartsWith("###"))
                        {
                            i = z;
                            break;
                        }
                    }
                }

                if (Contents[i].StartsWith("IF"))
                {
                    sb.CondStart = i+1;

                    cond = Contents[i].Replace("IF", "");
                    cond = cond.Substring(0, cond.LastIndexOf("THEN"));

                    sb.Expression = cond.Trim();

                    i++;

                    var r = GenerateStatements(ref i, Contents);
                    sb.OnTrue = r.Item1;
                    sb.OnFalse = r.Item2;
                    sb.CondEnd = i + 1;
                    this.CachedIf.Add(sb.Copy());
                    
                    if(closeOnFind)
                        return i + 1;
                }
            }
            return -1;
        }

        public Interpreter(string[] Contents, FLAGS flags)
        {
            //TODO: Remove 'Line' as 'line' is now an internal static variable
            file = Contents;
            this.variables = new Dictionary<string, Value>(1);
            function = new Dictionary<FuncMetaData, int>(1);
            Line = 0;
            //this.col = 0;
            this._flags = flags;
            this.callStack = new Stack<string>(1);
            this.CommentsRanges = new List<CommentRange>(2);
            
            int i = 0;

            if(file.Any(v => v.Contains("IF")))
            {
                bool hasElse = file.Any(v => v.Contains("ELSE"));

                this.CachedIf = new List<StatementBlock>(1);
                AddIfStatement(ref i, Contents);
            }
            
            char[] trimArray = new char[] { '\r', '\n', '\t', ' ' };
            int rS, rE = 0;
            for (int z = 0; z < file.Length; z++)
            {
                if (file[z].Trim(trimArray).StartsWith("###"))
                {
                    rS = z;
                    for (int y = z + 1; y < file.Length; y++)
                    {
                        if (file[y].Trim(trimArray).StartsWith("###"))
                        {
                            rE = y;
                            z = y;
                            break;
                        }
                    }

                    this.CommentsRanges.Add(new CommentRange { Start = rS, End = rE });
                    continue;
                }

                if (file[z].Trim(trimArray).StartsWith("#"))
                {
                    rS = z;

                    for (int y = z; y < file.Length; y++)
                    {
                        if (!file[y].Trim(trimArray).StartsWith("#"))
                        {
                            rE = y;
                            z = y;
                            break;
                        }
                    }

                    this.CommentsRanges.Add(new CommentRange { Start = rS, End = rE });
                    continue;
                }
            }
            
        }
   
        public static void SetupServer(string ip = null, int port = -1)
        {
            if(ip is null && port == 0)
            {
                server = new HSocket();
            }
            else
            {
                server = new HSocket(ip, port);
            }
        }

        public static FLAGS GetFlagsFromFile(string[] fileContents)
        {
            var flag_target = "#<DEBUG:";

            int ignore_count = 5;

            Interpreter.FLAGS my_flags = new Interpreter.FLAGS();

            // Check for any flags set in the files
            for (int i = 0; i < fileContents.Length; i++)
            {
                if (ignore_count == 0)
                    break;

                if (string.IsNullOrEmpty(fileContents[i]))
                    continue;

                if (fileContents[i].StartsWith(flag_target))
                {
                    // Attempt the strip the debug flag
                    var expression = new System.Text.RegularExpressions.Regex(@"#<DEBUG: (.+)>");

                    // ... See if we matched.
                    System.Text.RegularExpressions.Match match = expression.Match(fileContents[i]);

                    if (match.Success)
                    {
                        var val = match.Groups[1].Value;

                        if (val.Equals("PRINT_LINE_NUMBER"))
                        {
                            my_flags.DebugSendRequests = !my_flags.DebugSendRequests;
                            //var result = (my_flags.DebugSendRequests) ? "ON" : "OFF";
                            //Console.WriteLine($"PRINT_LINE_NUMBER IS {result}");
                            ignore_count++;
                            continue;
                        }

                        if (val.Contains("[") && val.Contains("]"))
                        {
                            var key = val.Substring(1, val.IndexOf(']') - 1);
                            var input = val.Substring(val.IndexOf('-') + 1);

                            if (my_flags.Inputs is null)
                                my_flags.Inputs = new Dictionary<string, string>(1);

                            my_flags.Inputs.Add(key, input);

                            ignore_count++;
                            continue;
                        }

                        Console.WriteLine($"IGNORING UNKOWN FLAG: {match.Groups[1].Value}");
                        break;
                    }
                }
                ignore_count = (ignore_count > 0) ? ignore_count -= 1 : 0;
            }

            return my_flags;
        }

    }
}
