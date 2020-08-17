using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
#pragma warning disable IDE0044 // Add readonly modifier
        internal static string[] file;
#pragma warning restore IDE0044 // Add readonly modifier
        public static bool executionHandled = false;

        public Dictionary<string, Value> variables { private set; get; }
        public Stack<string> callStack { private set; get; }

        internal static Dictionary<FuncMetaData, int> function { get; private set; }

        public static HSocket server { get; private set; }

        internal static int line;
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

        public Interpreter(string[] Contents, FLAGS flags)
        {
            //TODO: Remove 'Line' as 'line' is now an internal static variable
            file = Contents;
            this.variables = new Dictionary<string, Value>(1);
            function = new Dictionary<FuncMetaData, int>(1);
            line = 0;
            Line = 0;
            //this.col = 0;
            this._flags = flags;
            this.callStack = new Stack<string>(1);
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
