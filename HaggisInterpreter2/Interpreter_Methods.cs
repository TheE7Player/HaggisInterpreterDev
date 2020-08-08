using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace HaggisInterpreter2
{
    public partial class Interpreter
    {
        #region Logic
        private bool MoreThanOneFunction(string input)
        {
            var validFunctions = new string[] { "DECLEAR", "SET", "SEND", "RECEIVE" };
            var inArr = input.Trim().Split();
            var count = inArr.Count(x => validFunctions.Contains(x));
            return (count > 1);
        }


        private Value? RunMacro (Value call, bool returnValueBack)
        {
            Value funcReturn = new Value();

            if (call.OTHER is null)
                throw new Exception();

            if (call.OTHER.StartsWith("FN-"))
            {
                var fn_ref = function.Keys.First(x => x.Name == call.OTHER.Substring(3));

                callStack.Push(fn_ref.Name);

                int normalLine = line;

                int StartAt = function[fn_ref];
                int EndAt = fn_ref.FunctionEnd;

                var _variables = fn_ref.ArgValues;

                foreach (var item in _variables) { this.variables.Add(item.Key, item.Value); }

                string _line;
                bool Exit = false;
                string end_cond = (fn_ref.type == FuncMetaData.Type.FUNCTION) ? "END FUNCTION" : "END PROCEDURE";

                for (int i = StartAt; i <= EndAt; i++)
                {
                    if ((_line = GetNextLine((i - 1))) != null)
                    {
                        if (Exit || _line.Trim().StartsWith(end_cond))
                            break;

                        if (string.IsNullOrEmpty(_line))
                            continue;

                        if(fn_ref.type == FuncMetaData.Type.FUNCTION)
                        if(_line.Trim().StartsWith("RETURN"))
                        {
                            Exit = true;
                            funcReturn = variables["RETURNVAL"];
                            variables.Remove("RETURNVAL");
                            break;
                        }

                        Exit = _execute(_line.Split());
                    }
                }
                _line = null;
                foreach (var item in _variables) { this.variables.Remove(item.Key); }
                callStack.Pop();
                line = normalLine;

                if (fn_ref.type == FuncMetaData.Type.FUNCTION)
                    return funcReturn;
                else
                    return null;
            }
            return null;
        }

        private bool _execute(string[] executionLine)
        {
            string joinedExpression = string.Join(" ", executionLine);
            switch (executionLine[0])
            {
                case "DECLEAR":
                    Declear(executionLine, true);
                    return false;

                case "SET":
                    Declear(executionLine, false);
                    return false;

                case "SEND":
                    Send(joinedExpression);
                    return false;

                case "RECEIVE":
                    Receive(joinedExpression);
                    return false;

                case "IF":
                    If(joinedExpression);
                    return false;

                case "REPEAT":
                    Loop(true);
                    return false;

                case "WHILE":
                    Loop(false);
                    return false;

                case "PROCEDURE":
                    Function(joinedExpression);
                    return false;

                case "FUNCTION":
                    Function(joinedExpression, false);
                    return false;

                default:

                    if (string.IsNullOrEmpty(joinedExpression))
                        return false;

                    if(executionLine[0] == "RETURN")
                    {
                        var ep = string.Join(" ", executionLine.Skip(1));
                        var result = Expression.PerformExpression(variables, ep);
                        variables.Add("RETURNVAL", result);
                        return true;
                    }

                    try
                    {
                        var attempt = Expression.PerformExpression(variables, joinedExpression);

                        RunMacro(attempt, false);
                    }
                    catch (Exception _)
                    {
                        Error(_.Message, joinedExpression);
                        return true;
                    }
                    Error($"Expected a keyword, but got: {executionLine[0]} instead!", executionLine[0]);
                    return true;       
            }
        }

        private string GetNextLine(int ForceIndex = -1, bool Trim = true)
        {
            if (ForceIndex > -1)
            {
                if (ForceIndex > file.Length)
                    throw new Exception($"Supplied ForceIndex has succeeded past the max value (Interpreter called for {ForceIndex}, but max is: {file.Length})");

                line = ForceIndex;
                Line = ForceIndex;
            }

            while (line < file.Length)
            {
                if (file[line].Trim().StartsWith("#") || string.IsNullOrEmpty(file[line]))
                {
                    line++; Line++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (line == file.Length)
                return null;

            line++; Line++;
            return (Trim) ? file[line - 1].Trim() : file[line - 1];
        }

        public void Execute()
        {
            bool Exit = false;

            if (callStack.Count == 0)
                callStack.Push("script run");

            string line;
            while ((line = GetNextLine()) != null)
            {
                Exit = _execute(line.Split());

                if (Exit)
                    break;
            }

            callStack.Pop();
        }

        internal static int GetColumnFault(string toFind)
        {
            try
            {
                return file[line - 1].Trim().IndexOf(toFind);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static void Error(string message, string fault)
        {
            /* Order:
                [0] = Line fault
                [1] = Column fault
                [2] = Length of fault
            */
            int columnFault = GetColumnFault(fault);
            errorArea = new int[] { line, columnFault, fault.Length };
            executionHandled = true;
            throw new Exception(message);
        }

        #endregion

        private Value SetDefaultOrValid(string value, string target)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new Value(DefaultVal[target]);
            }

            if (target == "STRING" || target == "CHAR")
            {
                return (target == "STRING") ? new Value(value) : new Value((value.ToCharArray()[0]));
            }
            else if (target == "INTEGER")
            {
                if (Int32.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int i))
                {
                    return new Value(i);
                }
                else
                {
                    Column = GetColumnFault(value);
                    Error($"ASSIGN TYPE FAULT: Failed to assign an INTEGER operation with: {value}!", value);
                }
            }
            else if (target == "REAL")
            {
                if (Double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double i))
                {
                    return new Value(i);
                }
                else
                {
                    Column = GetColumnFault(value);
                    Error($"ASSIGN TYPE FAULT: Failed to assign an REAL operation with: {value}!", value);
                }
            }
            else if (target == "BOOLEAN")
            {
                if (Boolean.TryParse(value, out bool r))
                {
                    return new Value(r);
                }
                else
                {
                    Column = GetColumnFault(value);
                    Error($"ASSIGN TYPE FAULT: Failed to assign an BOOLEAN operation with: {value}!", value);
                }
            }
            return Value.Zero;
        }

        #region Keywords Functionality

        /// <summary>
        /// Method to assign a variable, either from SET or DECLEAR declaration
        /// </summary>
        /// <param name="information">The information passed in</param>
        /// <param name="isGlobal">True if 'DECLEAR', False if 'SET'</param>
        private void Declear(string[] information, bool isGlobal)
        {
            if (variables.ContainsKey(information[1]))
                Interpreter.Error($"VARIABLE {information[1]} ALREADY EXISTS/DECLARED", information[1]);

            if (isGlobal)
            {
                // Method is "DELCEAR"
                // Pattern: [DECLEAR] <VAR NAME> [AS] <DATA TYPE> ([INITIALLY AS] <Val>)?

                // we can now assume that [0] is correct, as this method wouldn't be called if it wasn't
                var var_name = information[1];

                // Check if 'AS' is used afterwards
                if (!information[2].Equals("AS"))
                {
                    Column = GetColumnFault(information[2]);
                    Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [AS] but got: {information[2]} instead!", information[2]);
                    return;
                }

                var var_type = information[3];
                if (!validTypes.Contains(var_type))
                {
                    Column = GetColumnFault(information[3]);
                    Error($"DECLEARATION FAULT: Declear requires a realiable data type: {information[3]} instead a valid type!", information[3]);
                    return;
                }

                Value value = new Value();

                if (information.Length == 4)
                {
                    value = new Value(DefaultVal[var_type]);
                }
                else
                {
                    // "INITALLY AS" has been called (Set a value)
                    if (!information[4].Equals("INITIALLY"))
                    {
                        Column = GetColumnFault(information[4]);
                        Error($"PATTERN FAULT: Pattern for [DECLEAR] should have followed [INITALLY] but got: {information[4]} instead!", information[4]);
                        return;
                    }

                    var express = Expression.GetExpression(information, 4);
                    var exp_type = Expression.GetExpressionType(express);

                    if (exp_type.Equals(Expression.ExpressionType.EXPRESSION))
                    {
                        Column = GetColumnFault(express);
                        var result = Expression.PerformExpression(this.variables, express);
                        variables.Add(var_name, result);
                        SendSocketMessage("variable_decl", $"{var_name}|{result}");
                        return;
                    }

                    value = SetDefaultOrValid(express, exp_type.ToString());
                }

                // Now we assign it
                variables.Add(var_name, value);
                SendSocketMessage("variable_decl", $"{var_name}|{value}");
            }
            else
            {
                // Method is "SET"
                // Pattern: [SET] <VAR NAME> [TO] <Val>
                var name = information[1];
                var express = information.Skip(3).ToArray();

                if (information[2] != "TO")
                {
                    Column = GetColumnFault(information[2]);
                    Error($"ASSIGNMENT FAULT: NEEDED \"TO\" TO ASSIGN A VARIABLE, GOT {information[2]} INSTEAD!", information[2]);
                }

                var result = Expression.PerformExpression(this.variables, String.Join(" ", express));

                if (!variables.ContainsKey(information[1]))
                {
                    if(!ReferenceEquals(result.OTHER, null))
                    {
                        if(result.OTHER.StartsWith("FN-"))
                        {
                            Value? r = RunMacro(result, true);

                            if (!ReferenceEquals(r, null))
                                result = (Value)r;
                        }
                    }

                    variables.Add(name, new Value(result));

                    SendSocketMessage("variable_decl", $"{name}|{result}");
                    name = null; express = null;
                }
                else
                {
                    variables[name] = new Value(result);
                    SendSocketMessage("variable_decl", $"{name}|{result}");
                    name = null; express = null;
                }
            }
        }

        private void Send(string express)
        {
            string[] ex = Expression.Evaluate(express).Select(f=>f.Trim()).ToArray();

            bool endsCorrectly = (ex[ex.Length - 2] == "TO" && ex[ex.Length - 1] == "DISPLAY");

            if (!endsCorrectly)
            {
                Column = GetColumnFault($"{ex[ex.Length - 2]} {ex[ex.Length - 1]}");
                Error($"Excepted \"TO DISPLAY\" ending, got {ex[ex.Length - 2]} {ex[ex.Length - 1]} instead", $"{ex[ex.Length - 2]} {ex[ex.Length - 1]}");
            }

            express = express.Replace("SEND", "");
            express = express.Replace("TO DISPLAY", "").Trim();

            Column = GetColumnFault(express);
            var exp = Expression.PerformExpression(this.variables, express);

            if (_flags.DebugSendRequests)
                Log($"LINE {line}: {exp}");
            else
                Log(exp.ToString());
        }

        private void Receive(string express)
        {
            //[2] FROM
            //[lastIndex] KEYBOARD
            string[] ex = Expression.Evaluate(express);
            string varName = ex[1];
            string varType = ex[4];

            if (!ex[2].Equals("FROM"))
            {
                Column = GetColumnFault(ex[2]);
                Error($"Excepted \"FROM\", got {ex[2]} instead", ex[2]);
            }

            if (!ex[ex.Length - 1].Equals("KEYBOARD"))
            {
                Column = GetColumnFault(ex[ex.Length - 1]);
                Error($"\"KEYBOARD\" is the only supported device at this time, got {ex[ex.Length - 1]} instead", ex[ex.Length - 1]);
            }

            // Good, it meant that the syntax pattern was correct!
            string input = string.Empty;

            if (!DefaultVal.Keys.Any(y => y.Equals(varType)))
            {
                Column = GetColumnFault(varType);
                Error($"\"{varType}\" isn't a recognisable data type for {varName}!", varType);
            }

            // Add the variable in if it hasn't already
            if (!variables.ContainsKey(varName)) variables.Add(varName, new Value(DefaultVal[varType]));

            if (!(_flags.Inputs is null))
            {
                if (_flags.Inputs.ContainsKey(varName))
                    input = _flags.Inputs[varName];
                else
                {
                    Column = GetColumnFault(varName);
                    Error($"{varName} isn't decleared or exists at time of execution", varName);
                }
            }
            else 
            {
                input = Input();
            }

            if (varType == "STRING")
            {
                variables[varName] = new Value(input);
            }

            if (varType == "CHARACTER")
            {
                variables[varName] = new Value(input[0]);
            }

            if (varType == "REAL")
            {
                // try to parse as double, if failed read value as string
                if (double.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    variables[varName] = new Value(d);
                else
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED REAL, GOT {input} INSTEAD", input);
                }
            }

            if (varType == "INTEGER")
            {
                if (int.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int i))
                    variables[varName] = new Value(i);
                else
                {
                    Column = GetColumnFault(input);
                    Error($"ERROR: EXPECTED INTEGER, GOT {input} INSTEAD", input);
                }
            }
            SendSocketMessage("variable_inpt", $"{varName}|{input}");
        }

        private void If(string expression)
        {       
            #region Verticle If Statement

            if (expression.StartsWith("IF") && expression.EndsWith("END IF") && expression.Contains("THEN"))
            {
                Column = GetColumnFault("IF");
                string condition_expression = expression.Substring(3, expression.IndexOf("THEN") - 3).Trim();
                Column = GetColumnFault(condition_expression);
                var result = Expression.PerformExpression(this.variables, condition_expression);
                string trueExpression;
                string falseExpression;

                //Avoid evaluation on horiz. IF statment if it evaluates to false (No need to waste cpu cycles)
                if (!expression.Contains("ELSE") && result.BOOLEAN == false)
                    return;

                int tIndex = expression.IndexOf("THEN") + 4;
                int tIndexEnd = expression.LastIndexOf("ELSE");
                int fIndexEnd = expression.LastIndexOf("END");

                if (expression.Contains("ELSE"))
                {
                    trueExpression = expression.Substring(tIndex, tIndexEnd - tIndex).Trim();

                    if (MoreThanOneFunction(trueExpression))
                    {
                        Column = GetColumnFault(trueExpression);
                        Error("EXPRESSION FAULT: HAD MORE THAN 1 STATEMENT IN A SINGLE LINE, USE VERTICAL IF STATEMENT INSTEAD", trueExpression);
                        return;
                    }

                    falseExpression = expression.Substring(tIndexEnd + 4, fIndexEnd - (tIndexEnd + 4)).Trim();

                    if (MoreThanOneFunction(falseExpression))
                    {
                        Column = GetColumnFault(falseExpression);
                        Error("EXPRESSION FAULT: HAD MORE THAN 1 STATEMENT IN A SINGLE LINE, USE VERTICAL IF STATEMENT INSTEAD", trueExpression);
                        return;
                    }

                    if (result.BOOLEAN == true)
                    { _execute(trueExpression.Split()); return; }
                    else
                    { _execute(falseExpression.Split()); return; }
                }
                else
                    trueExpression = expression.Substring(tIndex, fIndexEnd - tIndex).Trim();

                _execute(trueExpression.Split());
            }

            #endregion Verticle If Statement

            #region Horizontal If Statement

            if (expression.StartsWith("IF") && expression.EndsWith("THEN"))
            {
                //TODO: Handle if 'ELSE' or 'END IF' isn't included?

                Column = GetColumnFault("IF");
                string condition_expression = expression.Substring(3, expression.IndexOf("THEN") - 3).Trim();
                Column = GetColumnFault(condition_expression);
                var result = Expression.PerformExpression(this.variables, condition_expression);

                // If false, skip to when we reach `ELSE` case
                if (result.BOOLEAN == false)
                {
                    // Skip the lines till we hit 'ELSE'
                    string _l;
                    while (!(_l = GetNextLine()).Contains("ELSE")) { }

                    if (_l.StartsWith("ELSE IF"))
                    {
                        _execute(_l.Trim().Substring(5).Split());
                    }
                    else
                    {
                        while ((_l = GetNextLine()) != "END IF")
                            _execute(_l.Trim().Split());

                        GetNextLine();
                    }
                }
                else
                {
                    // Result is true
                    string _l;
                    while (!(_l = GetNextLine()).Contains("ELSE"))
                    {
                        if (_l == "END IF")
                            return;

                        _execute(_l.Trim().Split());
                    }

                    while ((_ = GetNextLine()) != "END IF") { }
                }
            }

            #endregion Horizontal If Statement
        }

        private void Loop(bool isRepeatLoop = true)
        {

            int iterStart = line;
            string condition = string.Empty;

            bool Exit = false;

            if(isRepeatLoop)
            {
                // REPEAT loop
                string _line;
                while ((_line = GetNextLine()) != null)
                {
                    if (_line.StartsWith("UNTIL"))
                    {
                        if(object.ReferenceEquals(condition, string.Empty))
                            condition = _line.Substring(6);

                        Value result = Expression.PerformExpression(variables, condition);

                        if (!result.BOOLEAN)
                        {
                            break;
                        }
                        else
                        {
                            line = iterStart;
                            Line = line;
                        }
                    }
                    else 
                    {  
                        Exit = _execute(_line.Split());

                        if (Exit)
                            break;
                    }
                }              
            }
            else
            {
                // WHILE loop
                string _line;

                if (!(file[line - 1].EndsWith("DO")))
                {
                    if(!(file[line - 1].StartsWith("DO")))
                    {
                        var words = file[line - 1].Split();
                        Column = GetColumnFault(words[words.Length - 1]);
                        Error("Missing expression ender for WHILE loop - Please a 'DO' after expression", words[words.Length - 1]);
                    }
                }

                condition = file[line-1].Substring(6);
                condition = condition.Substring(0, condition.Length - 2).Trim();

                Value result = Expression.PerformExpression(variables, condition);

                if (!result.BOOLEAN)
                    return;

                while ((_line = GetNextLine()) != null)
                {
                    if (_line.StartsWith("END WHILE"))
                    {
                        result = Expression.PerformExpression(variables, condition);

                        if (!result.BOOLEAN)
                        {
                            break;
                        }
                        else
                        {
                            line = iterStart;
                            Line = line;
                        }
                    }
                    else
                    {
                        Exit = _execute(_line.Split());

                        if (Exit)
                            break;
                    }
                }
            }

        }

        private void Function(string express, bool isProcedure = true)
        {
            string[] _dmeta = Expression.Evaluate(express);
            string funcName = _dmeta[1];

            express = express.Substring(express.IndexOf('(') + 1);
            express = express.Substring(0, express.IndexOf(')'));

            var _var = express.Split(',').Select(_ => _.Trim()).ToArray();

            Value val;
            List<string> pTypes = new List<string>(1);
            Dictionary<string, Value> argVal = new Dictionary<string, Value>(1);

            foreach (var param in _var)
            {
                var data = param.Split(' ');

                if (!validTypes.Contains(data[0]))
                    Error("Unkown Data type given", param);

                val = SetDefaultOrValid("", data[0]);
                pTypes.Add(data[0]);
                argVal.Add(data[1], val);
            }

            if (!isProcedure)
            {
                if (_dmeta[_dmeta.Length - 2] != "RETURNS")
                {
                    int funcEnd = _dmeta.ToList().IndexOf(")");
                    string[] newMeta = new string[funcEnd];
                    Array.Copy(_dmeta, 1, newMeta, 0, funcEnd);
                    string func = string.Join(" ", newMeta);
                    string procedure_name = $"PROCEDURE {func}";
                    Error($"DECLEARLESS FUNCTION, ADD A RETURN TYPE OR CHANGE IT TO A PROCEDURE INSTEAD -> {procedure_name} ", _dmeta[_dmeta.Length - 2]);
                }

                if (!validTypes.Contains(_dmeta[_dmeta.Length - 1]))
                {
                    Error($"UNKNOWN RETURN TYPE FOR THIS FUNCTION: { _dmeta[_dmeta.Length - 1]}", _dmeta[_dmeta.Length - 1]);
                }

                Enum.TryParse<ValueType>(_dmeta[_dmeta.Length - 1], out ValueType _t);

                function.Add(new FuncMetaData
                {
                    Name = funcName,
                    type = FuncMetaData.Type.FUNCTION,
                    ArgTypes = pTypes.ToArray(),
                    returnType = _t,
                    ArgValues = argVal
                }, line + 1);

            }
            else 
            { 
                function.Add(new FuncMetaData
                {
                    Name = funcName,
                    type = FuncMetaData.Type.PROCEDURE,
                    ArgTypes = pTypes.ToArray(),
                    ArgValues = argVal
                }, line + 1); 
            }

            pTypes = null; argVal = null; _var = null; _dmeta = null;

            string _line = "";
            string cmp_line = (isProcedure) ? "END PROCEDURE" : "END FUNCTION";
            while ((_line = GetNextLine()) != null)
            {
                if (_line.StartsWith(cmp_line))
                {
                    var k = function.First(x => x.Key.Name == funcName).Key;
                    var v = function.First(x => x.Key.Name == funcName).Value;
                    function.Remove(k);

                    k.FunctionEnd = line;
                    function.Add(k, v);
                    break;
                }
            }

            SendSocketMessage("func_dlcr", $"{funcName}|{(isProcedure?'P':'F')}");
            funcName = null;
            GC.Collect();
        }

        #endregion
    }
}