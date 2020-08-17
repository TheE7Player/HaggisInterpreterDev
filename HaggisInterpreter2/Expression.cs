using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace HaggisInterpreter2
{
    internal static class Expression
    {
        public static char[] validOperations = new char[] { '+', '/', '*', '-', '(', ')', '&', '!', '=', '>', '<'};
        public static string[] validFunctions = new string[] { "Lower", "Upper", "Trim", "Title" };
        public static string[] validComparisons = new string[] {">", "<", "!=", "NOT", "AND", "OR", "<=", ">=", "<>", "!", "="};
        public enum ExpressionType
        {
            /// <summary>
            /// LITERAL - Constant number, expressed at compile time: 0, 200, "Hey"
            /// </summary>
            LITERAL,

            /// <summary>
            /// EXPRESSION - Modification of one or more values at run time (Comparsion, Concat, Addition etc)
            /// </summary>
            EXPRESSION
        }

        #region Expression Logic

        public static string GetExpression(string[] values, int exprStartIndex)
        {
            exprStartIndex++;
            var sb = new StringBuilder();

            for (int i = exprStartIndex; i < values.Length; i++)
                if (i < (values.Length - 1))
                    sb.Append($"{values[i]} ");
                else
                    sb.Append(values[i]);

            return sb.ToString();
        }

        public static ExpressionType GetExpressionType(object val)
        {
            if (val.ToString().ToCharArray().Any(x => validOperations.Contains(x)))
                return ExpressionType.EXPRESSION;

            return ExpressionType.LITERAL;
        }

        public static string[] Evaluate(string Expression)
        {
            // Clean up the string
            var iter = Expression.Trim().ToCharArray();
            var sb = new StringBuilder();
            List<string> output = new List<string>(2);
            try
            {
                // 34 -> "
                // 32 -> (whitespace)
                for (int i = 0; i < iter.Length; i++)
                {
                    // Is current char number '32'
                    if ((int)iter[i] == 32)
                    {
                        if (sb.Length > 0)
                        {
                            output.Add(sb.ToString());
                            sb.Clear();
                        }

                        if(iter[i-1] != '"' || (iter[i] == '&' || iter[i+1] == '&'))
                           continue;
                    }

                    if ((int)iter[i] == 34 || (int)iter[i] == 39)
                    {
                        i++;
                        output.Add(BuildQuote(iter, ref i, ((int)iter[i-1] == 34) ?'"':'\''));
                        continue;
                    }
                    else
                    {
                        //40 = (, 41 == )
                        if ((int)iter[i] == 40 || (int)iter[i] == 41)
                        {
                            if (sb.Length > 0)
                            {
                                output.Add(sb.ToString());
                                sb.Clear();

                                sb.Append(iter[i]);
                                output.Add(sb.ToString());
                                sb.Clear();
                            }
                            else
                            {
                                sb.Append(iter[i]);
                                output.Add(sb.ToString());
                                sb.Clear();
                            }
                        }
                        else
                        {
                            if (validOperations.Contains(iter[i]))
                            {
                                bool ignoreNegative = false;

                                // If not the end of the array
                                if((i + 1) < iter.Length - 1) { if(iter[i] == '-' && !char.IsDigit(iter[i + 1])) { ignoreNegative = true; } }

                                // Deal with numbers with negative sign
                                if (iter[i] == '-' && !ignoreNegative)
                                {
                                    if (sb.Length > 0)
                                        output.Add(sb.ToString());
                                    sb.Clear();
                                    sb.Append(iter[i]);
                                    i++;
                                    while (i < (iter.Length - 1))
                                    {
                                        if (char.IsDigit(iter[i]) || iter[i] == '.')
                                        { sb.Append(iter[i]); }
                                        i++;
                                    }

                                    if (sb.ToString() == "-")
                                    {
                                        if (iter[i - 1] == ' ') 
                                        {
                                            output.Add(sb.ToString());
                                            output.Add(iter[i].ToString());
                                        }
                                        else 
                                        { 
                                            sb.Append(iter[i]);
                                            output.Add(sb.ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (sb.Length > 0)
                                            output.Add(sb.ToString());
                                        output.Add(iter[i].ToString());
                                    }

                                    sb.Clear();
                                }
                                else
                                {
                                    if (sb.Length > 0)
                                        output.Add(sb.ToString());

                                    output.Add(iter[i].ToString());
                                    sb.Clear();
                                }
                            }
                            else
                            {
                                // There is a chance that a ',' could be here
                                if(iter[i] == ',')
                                {
                                    if (sb.Length > 0)
                                        output.Add(sb.ToString());
                                    sb.Clear();
                                }

                                sb.Append(iter[i]);
                            }
                        }
                    }
                }

                if (sb.Length > 0)
                {
                    output.Add(sb.ToString());
                    sb.Clear();
                }
            }
            catch (Exception _)
            {
                Interpreter.Error(_.Message, "");
            }
            finally
            {
                iter = null;
                sb = null;
            }

            return output.ToArray();
        }

        private static string BuildQuote(char[] text, ref int currentIndex, char QuoteChar = '"')
        {
            var sb = new StringBuilder();
            while (currentIndex < (text.Length - 1 ))
            {
                if (text[currentIndex] != QuoteChar)
                    sb.Append(text[currentIndex]);
                else
                    break;

                currentIndex++;
            }
            return sb.ToString();
        }

        private static Value DoExpr(Value l, string op, Value r)
        {

            if (l.Type != r.Type)
            {
                // promote one value to higher type

                // Ignore if the left hand is a string
                bool lOptIgnore = (l.Type == ValueType.STRING || l.Type == ValueType.CHARACTER);
                if (lOptIgnore == false)
                {
                    if (r.Type != ValueType.STRING)
                        if (l.Type > r.Type)
                            r = r.Convert(l.Type);
                        else
                            l = l.Convert(r.Type);
                }
            }

            if (op.Equals("+"))
            {
                if (l.Type == ValueType.REAL)
                    return new Value(l.REAL + r.REAL);

                if (l.Type == ValueType.INTEGER)
                    return new Value(l.INT + r.INT);

                if (l.Type == ValueType.CHARACTER || l.Type == ValueType.STRING)
                   HaggisInterpreter2.Interpreter.Error("Left opp cannot use + to join text together! Use '&' instead", l.ToString());

                if (r.Type == ValueType.CHARACTER || r.Type == ValueType.STRING)
                    HaggisInterpreter2.Interpreter.Error("Right opp cannot use + to join text together! Use '&' instead", r.ToString());
            }
            else if (op.Equals("&"))
            {
                if (l.Type == ValueType.STRING || r.Type == ValueType.STRING)
                    return new Value(l.ToString() + r.ToString());

                // You cannot append l char, need to convert to string (C# Does that automatically for us)
                if (l.Type == ValueType.CHARACTER || r.Type == ValueType.CHARACTER)
                    if (l.Type == ValueType.CHARACTER)
                        return new Value(l.CHARACTER + r.ToString());
                    else if (r.Type == ValueType.CHARACTER)
                        return new Value(l.ToString() + r.CHARACTER);
                    else
                        HaggisInterpreter2.Interpreter.Error($"PROBLEM: Issue with managing characters on left or right hand side ({l},{r})", op);
            }
            else if (op.Equals("="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL);

                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING);

                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN);

                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT);

                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER);
                }
            }
            else if (op.Equals("!="))
            {
                switch (l.Type)
                {
                    case ValueType.REAL:
                        return new Value(l.REAL == r.REAL);

                    case ValueType.STRING:
                        return new Value(l.STRING == r.STRING);

                    case ValueType.BOOLEAN:
                        return new Value(l.BOOLEAN == r.BOOLEAN);

                    case ValueType.INTEGER:
                        return new Value(l.INT == r.INT);

                    case ValueType.CHARACTER:
                        return new Value(l.CHARACTER == r.CHARACTER);
                }
            }
            else
            {
                if (l.Type == ValueType.STRING)
                    HaggisInterpreter2.Interpreter.Error("Cannot perform requested \"BinOp\" on \"STRING\"", l.ToString());

                if (l.Type == ValueType.CHARACTER)
                    HaggisInterpreter2.Interpreter.Error("Cannot perform requested \"BinOp\" on \"CHARACTER\"", l.ToString());

                if (l.Type == ValueType.REAL)
                    switch (op)
                    {
                        case "-": return new Value(l.REAL - r.REAL);
                        case "*": return new Value(l.REAL * r.REAL);
                        case "/": return new Value(l.REAL / r.REAL);
                        //case Token.Caret: return new Value(Math.Pow(l.REAL, r.REAL));
                        case "<": return new Value(l.REAL < r.REAL);
                        case ">": return new Value(l.REAL > r.REAL);
                        case "<=": return new Value((l.REAL <= r.REAL));
                        case ">=": return new Value(l.REAL >= r.REAL);
                        case "AND": return new Value((l.REAL != 0) && (r.REAL != 0));
                        case "OR": return new Value((l.REAL != 0) || (r.REAL != 0));
                        case "!=": return new Value((l.REAL != 0) != (r.REAL != 0));
                    }

                if (l.Type == ValueType.INTEGER)
                    switch (op)
                    {
                        case "-": return new Value(l.INT - r.INT);
                        case "*": return new Value(l.INT * r.INT);
                        case "/": return new Value(l.INT / r.INT);
                        //case Token.Caret: return new Value(Math.Pow(l.INT, r.INT));
                        case "<": return new Value(l.INT < r.INT);
                        case ">": return new Value(l.INT > r.INT);
                        case "<=": return new Value(l.INT <= r.INT);
                        case ">=": return new Value(l.INT >= r.INT);
                        case "AND": return new Value((l.INT != 0) && (r.INT != 0));
                        case "OR": return new Value((l.INT != 0) || (r.INT != 0));
                        case "!=": return new Value((l.INT != 0) != (r.INT != 0));
                    }

                if (l.Type == ValueType.BOOLEAN)
                    switch (op)
                    {
                        case "AND": return new Value((l.BOOLEAN != false) && (r.BOOLEAN != false));
                        case "OR": return new Value((l.BOOLEAN != false) || (r.BOOLEAN != false));
                        case "NOT": return new Value((l.BOOLEAN != false) != (r.BOOLEAN != false));
                    }
            }

            HaggisInterpreter2.Interpreter.Error("Unknown binary operator.", op);
            return Value.Zero;
        }

        #endregion Expression Logic

        #region Expression Operations

        private static string[] StringGapFix(string[] target)
        {
            var newarr = new List<string>(2);
            var sb = new StringBuilder();
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i].Length != 0) 
                {
                    if (sb.Length > 0)
                    { newarr.Add(sb.ToString()); sb.Clear(); }

                    newarr.Add(target[i]); continue; 
                }

                if (target[i].Length == 0)
                    sb.Append(' ');
            }

            sb = null;
            return newarr.ToArray();
        }

        public static Value PerformExpression(Dictionary<string, Value> vals, string expression, bool alreadyEvaluated = false)
        {
            //TODO: FIX EMPTY SPACES IN TEXT STRING
            bool notWrapper = false;

            string[] exp;
            if (alreadyEvaluated)
                exp = expression.Split();
            else
                exp = Evaluate(expression.ToString());

            if (expression.StartsWith("NOT"))
            {
                notWrapper = true;
                var new_exp = exp.ToList();

                new_exp.RemoveAt(0); // Removes the NOT
                new_exp.RemoveAt(0); // Removes the NOT Left Bracket
                new_exp.RemoveAt(new_exp.Count - 1); // Removes the NOT Right Bracket

                exp = new_exp.ToArray();
                new_exp = null;
            }

            // Any gaps left in a string causes issues - Lets resolve that.
            if(exp.Any(g => g.Length == 0)) { exp = StringGapFix(exp); }

            List<IBlock> blocks = BlockParser.GenerateBlocks(exp, vals);

            // If its all just text, we just ammend it
            if(!blocks.Any(x => x.BinaryOp != ""))
            {
                int allText = blocks.Where(x=>x.blockType == BlockType.Text).Distinct().Count();

                if(allText == blocks.Count)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < blocks.Count; i++)
                    {
          
                        sb.Append(blocks[i].Value.ToString());

                        if(i < (blocks.Count - 1))
                            sb.Append(" ");
                    }
                    blocks = new List<IBlock> { new Block { Value = new Value(sb.ToString(), true) } };
                }
            }

            bool sortByOrderNumber = false;
            if(blocks.Count > 1)
            {
                var sortedLevel = BlockParser.SortByOrder(blocks);

                List<IBlock> lvlList;
                int HighestIndex = 0;
                int currentLevel = 0;
                foreach (var lvl in sortedLevel)
                {
                    lvlList = lvl.Value;
                    currentLevel = lvl.Key;
                    HighestIndex = lvlList.Count - 1;

                    if(!sortByOrderNumber){ sortByOrderNumber = true;}
                    
                    // List is always in order first iteration, but not after second onwrds
                    if(sortByOrderNumber)
                        lvlList = lvlList.OrderBy(o => o.OrderNumber).ToList();
                                     
                    if (string.IsNullOrEmpty(lvlList[HighestIndex - 1].BinaryOp))
                    {
                        if(lvlList[HighestIndex - 1].BinaryOp == null && (lvlList[HighestIndex].BinaryOp != null && lvlList[HighestIndex].blockType == BlockType.Text))
                        {
                            lvlList[HighestIndex - 1].BinaryOp = lvlList[HighestIndex].BinaryOp;
                        }
                        else
                        if(lvlList[0].blockType == BlockType.BinOp )
                        {
                            // We need to fix the order, The newest value (Highest index needs to be on top, at index 0)
                            var newLeft = lvlList[HighestIndex];   
                            lvlList.RemoveAt(HighestIndex);
                            lvlList.Insert(0, newLeft);

                            // We need to remove the operator!
                            lvlList[0].BinaryOp = lvlList[1].BinaryOp;
                            lvlList.RemoveAt(1);

                            // Reassign the max index as it was based on 3(ish) items and not 2
                            HighestIndex = lvlList.Count - 1;
                        }
                        else
                            Interpreter.Error($"Cannot do an operation with the given BinOP, \"{ lvlList[HighestIndex - 1].BinaryOp }\"!", lvlList[HighestIndex - 1].BinaryOp);           
                    }

                    // Handle if there is an BinOP by itself
                    int location;
                    bool locForceBreak = false;
                    while(lvlList.Any(item => item.blockType == BlockType.BinOp))
                    {
                        location = lvlList.FindIndex(item => item.blockType == BlockType.BinOp);

                        // Test if "Above and Below" check is possible
                        if ((location - 1) >= 0 && (location + 1) <= lvlList.Count - 1)
                        {
                            // It is possible, time to convert it into an ConditionBlock

                            Value _left;
                            Value _right;
                            string _op;

                            _op = lvlList[location].BinaryOp;

                            if (GetBlockType(lvlList[location - 1]) == "Function")
                                _left = FuncEval(lvlList[location - 1], false, vals);
                            else
                                _left = (vals.ContainsKey(lvlList[location - 1].Value.ToString())) ? vals[lvlList[location - 1].Value.ToString()] : lvlList[location - 1].Value;

                            if (GetBlockType(lvlList[location + 1]) == "Function")
                                _right = FuncEval(lvlList[location + 1], false, vals);
                            else
                                _right = (vals.ContainsKey(lvlList[location + 1].Value.ToString())) ? vals[lvlList[location + 1].Value.ToString()] : lvlList[location + 1].Value;

                            lvlList.RemoveAt(location + 1);
                            lvlList.RemoveAt(location);
                            lvlList.RemoveAt(location - 1);
                            lvlList.Add(new ConditionBlock { OrderLevel = currentLevel, OrderNumber = location, Left = _left, CompareOp = _op, Right = _right, blockType = BlockType.Expression});

                            HighestIndex = ((lvlList.Count - 1) > 0) ? HighestIndex = lvlList.Count - 1 : 0;
                        }
                        else
                        {
                            if(!lvlList.Any(z => z.blockType == BlockType.BinOp) && lvlList.Count > 2)
                                Interpreter.Error("Unable to modify block to suit BinOP Block", "");

                            //Send the op and text to a lower level
                            var bin_op = lvlList.First(l => l.blockType == BlockType.BinOp);
                            var b = lvlList.First(l => l.blockType == BlockType.Text || l.blockType == BlockType.Literal);

                            b.BinaryOp = bin_op.BinaryOp;

                            int fixed_order = (bin_op.OrderNumber < b.OrderNumber) ? bin_op.OrderNumber : b.OrderNumber;
                            b.OrderNumber = fixed_order;
                            b.OrderLevel -= 1;
                            lvlList.RemoveAt(1);
                            lvlList.RemoveAt(0);

                            int newsortedLevel = (currentLevel - 1 > 0) ? currentLevel - 1 : 0;

                            sortedLevel[newsortedLevel].Add(b);

                            locForceBreak = true;
                            break;
                        }
                    }

                    if (locForceBreak)
                        continue;

                    // Cache the valve(s) for better performance
                    Value left = Value.Zero;
                    Value right = Value.Zero;
                    Value eval = Value.Zero;
                    string op = string.Empty;

                    int orginalOrder = 0;

                    while (lvlList.Count > 0)
                    {

                        if (lvlList[HighestIndex].GetType().Name == "ConditionBlock")
                        {
                            ConditionBlock cb = lvlList[HighestIndex] as ConditionBlock;

                            // First check if any variables here are stored

                            // Problem: If variable is char, covert to string

                            if(cb.Left.Type == ValueType.CHARACTER)
                            {
                                cb.Left = cb.Left.Convert(ValueType.STRING);
                            }

                            if(cb.Right.Type == ValueType.CHARACTER)
                            {
                                cb.Right = cb.Right.Convert(ValueType.STRING);
                            }

                            if (cb.Left.Type == ValueType.STRING)
                                cb.Left = (vals.ContainsKey(cb.Left.STRING)) ? vals[cb.Left.STRING] : cb.Left;

                            if (cb.Right.Type == ValueType.STRING)
                                cb.Right = (vals.ContainsKey(cb.Right.STRING)) ? vals[cb.Right.STRING] : cb.Right;

                            var _eval = DoExpr(cb.Left, cb.CompareOp, cb.Right);

                            lvlList.RemoveAt(HighestIndex);
                            HighestIndex = ((lvlList.Count - 1) > 0) ? HighestIndex = lvlList.Count - 1 : 0;

                            if (string.IsNullOrEmpty(cb.BinaryOp))
                                lvlList.Add(new Block { Value = _eval, OrderNumber = cb.OrderNumber, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                            else
                                lvlList.Insert(HighestIndex, new Block { Value = _eval, OrderNumber = cb.OrderNumber, OrderLevel = currentLevel, BinaryOp = cb.BinaryOp, blockType = BlockType.Literal });

                            // If we managed to get all ConditionBlocks into Blocks, we reset back to normal (As an index error will happen if we don't!)
                            bool ConversionNotComplete = lvlList.Any(x => x.GetType().Name == "ConditionBlock") || (lvlList.Count == 1 && lvlList[0].GetType().Name == "Block");

                            // Refix back to normal HighestIndex
                            if (!ConversionNotComplete)
                                HighestIndex = lvlList.Count - 1;

                            continue;
                        }
                        else if (lvlList[HighestIndex].GetType().Name == "FuncBlock")
                        {
                            FuncBlock fb = lvlList[HighestIndex] as FuncBlock;

                            //TODO: ID array of args (Only single args at the moment)
                            string args = "";

                            if(!(fb.Args is null))
                            if (fb.Args.Length == 1)
                            {
                                args = fb.Args[0];
                                args = (vals.ContainsKey(args)) ? vals[args].ToString() : fb.Args[0];
                            }
                            else 
                            { 
                                args = string.Join(",", fb.Args);
                                Interpreter.Error("Multiple arguments aren't supported in this current build - Please wait till this gets optimised!", args);
                            }

                            var _eval = FuncExtensions(fb.FunctionName, args);

                            lvlList.RemoveAt(HighestIndex);
                            HighestIndex = ((lvlList.Count - 1) > 0) ? HighestIndex = lvlList.Count - 1 : 0;

                            if (string.IsNullOrEmpty(fb.BinaryOp))
                                lvlList.Add(new Block { Value = _eval, OrderNumber = fb.OrderNumber, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                            else
                                lvlList.Insert(HighestIndex, new Block { Value = _eval, OrderNumber = fb.OrderNumber, OrderLevel = currentLevel, BinaryOp = fb.BinaryOp, blockType = BlockType.Literal });

                            // If we managed to get all ConditionBlocks into Blocks, we reset back to normal (As an index error will happen if we don't!)
                            bool ConversionNotComplete = lvlList.Any(x => x.GetType().Name == "FuncBlock");

                            // Refix back to normal HighestIndex
                            if (!ConversionNotComplete)
                                HighestIndex = lvlList.Count - 1;

                            continue;
                        }
                        else
                        {

                            // No point evaluation as there is a block with a potential value
                            if (lvlList.Count == 1 && lvlList[0].GetType().Name == "Block")
                            {
                                var result = lvlList[0].Value;
                                if (notWrapper)
                                {
                                    if (result.Type != ValueType.BOOLEAN)
                                        Interpreter.Error($"Cannot use the NOT wrapper with {result.Type}", result.ToString());

                                    result.BOOLEAN = !result.BOOLEAN;
                                }
                                return result;
                            }

                            try
                            {
                                if (lvlList[HighestIndex - 1].blockType == BlockType.Variable)
                                    left = vals[lvlList[HighestIndex - 1].Value.ToString()];
                                else if (lvlList[HighestIndex - 1].blockType == BlockType.Function)
                                    left = FuncEval(lvlList[HighestIndex - 1], notWrapper, vals);
                                else
                                    left = lvlList[HighestIndex - 1].Value;
                            }
                            catch (Exception)
                            {
                                left = lvlList[HighestIndex - 1].Value;
                            }

                            op = lvlList[HighestIndex - 1].BinaryOp;

                            try
                            {
                                if (lvlList[HighestIndex].blockType == BlockType.Variable)
                                    right = vals[lvlList[HighestIndex].Value.ToString()];
                                else if (lvlList[HighestIndex].blockType == BlockType.Function)
                                    left = FuncEval(lvlList[HighestIndex], notWrapper, vals);
                                else
                                    right = lvlList[HighestIndex].Value;
                            }
                            catch (Exception)
                            {
                                right = lvlList[HighestIndex].Value;
                            }

                            orginalOrder = lvlList[HighestIndex - 1].OrderNumber;

                            lvlList.RemoveAt(HighestIndex);
                            lvlList.RemoveAt(HighestIndex - 1);

                        }

                        // Move up the data type to allow doubles with floats not to be assigned "0"
                                           
                        /*if (left.Type == ValueType.INTEGER)
                            left = left.Convert(ValueType.REAL);

                        if (right.Type == ValueType.INTEGER)
                            right = right.Convert(ValueType.REAL);*/

                        eval = DoExpr(left, op, right);

                        if (lvlList.Count == 0)
                        {
                            if((currentLevel - 1) == -1)
                            {
                                if(notWrapper)
                                {
                                    if (eval.Type != ValueType.BOOLEAN)
                                        Interpreter.Error($"Cannot use the NOT wrapper with {eval.Type}", eval.ToString());

                                    eval.BOOLEAN = !eval.BOOLEAN;
                                    return eval;
                                }

                                return eval;
                            }

                            // That means we've reach all the expressions needed for this current level
                            int newLevel = BlockParser.GetHighestOrder(sortedLevel[currentLevel - 1]);

                            sortedLevel[currentLevel - 1].Add(new Block { Value = eval, OrderNumber = orginalOrder, OrderLevel = newLevel, BinaryOp = String.Empty, blockType = BlockType.Literal});
                            continue;
                        }

                        // Ammend it as there are still some more operations to do
                        
                        //sortedLevel[currentLevel] ?? Relook this part?!
                        lvlList.Add(new Block { Value = eval, OrderNumber = orginalOrder, OrderLevel = currentLevel, BinaryOp = String.Empty, blockType = BlockType.Literal });
                        HighestIndex = lvlList.Count - 1;
                    }
                }
            }

            if (GetBlockType(blocks[0]) == "Condition")
            {
                var b = blocks[0] as ConditionBlock;
                bool notOperater = (b.CompareOp == "!=" || b.CompareOp == "<>");

                if (!notWrapper && notOperater)
                    notWrapper = true;

                return CondEval(blocks[0], notWrapper, vals);
            }
            else if (GetBlockType(blocks[0]) == "Function")
            {
                var fn = blocks[0] as FuncBlock;
                if (isPreDefined(fn.FunctionName))
                {
                    var result = PreDefinedFunctions(fn.FunctionName, expression, vals);
                    return result;
                }
                else
                    return FuncEval(blocks[0], notWrapper, vals);
            }
            else
                return blocks[0].Value;         
        }

        #region Block Evaluations
        private static string GetBlockType(IBlock block)
        {
            return block.GetType().Name switch
            {
                "ConditionBlock" => "Condition",
                "FuncBlock" => "Function",
                _ => string.Empty,
            };
        }

        private static Value FuncEval(IBlock block, bool isNotOperator, Dictionary<string, Value> vals)
        {

            FuncBlock fb = block as FuncBlock;
            FuncMetaData meta;
            bool isOverride = false;

            if(isPreDefined(fb.FunctionName))
            {
                var result = PreDefinedFunctions(fb.FunctionName, string.Join(",", fb.Args), vals);

                if (ReferenceEquals(result, Value.Zero))
                {
                    Interpreter.Error($"Problem identifying the follow pseudo function: {fb.FunctionName}\nHere are the list of them: {string.Join(", ", availableFunctions)}", fb.FunctionName);
                }

                return result;
            }

            try
            {
                meta = availableFunctions.First(x => x.Name == fb.FunctionName);
                isOverride = availableFunctions.Count(x => x.Name == fb.FunctionName) > 1;
            }
            catch (Exception)
            {
                meta = Interpreter.function.FirstOrDefault(x => x.Key.Name == fb.FunctionName).Key;

                if(meta.Name is null && meta.ArgTypes is null && meta.ArgValues is null)
                    Interpreter.Error($"ERROR FINDING {fb.FunctionName} - Either no declared, spelled wrong or is called before declaration", fb.FunctionName);
            }

            //TODO: FIXED EXPRESSIONS IN ARRAY
            string args = null;

            if(!(fb.Args is null))
            {
                if (!(meta.ArgValues is null))
                    if (fb.Args.Length != meta.ArgValues.Count())
                        Interpreter.Error($"NON-MATCHING PARAMETERS, EXPECTED {meta.ArgValues.Count()} BUT GOT {fb.Args.Length} INSTEAD", meta.Name);

                if (fb.Args.Length == 1)
                {
                    args = fb.Args[0];
                    args = (vals.ContainsKey(args)) ? vals[args].ToString() : fb.Args[0];

                    Value val;

                    if (args.Split(' ').Any(c => validOperations.Contains(c[0]) || validComparisons.Contains(c)))
                        val = PerformExpression(vals, args, true);
                    else
                        val = new Value(args, true);

                    if (isOverride)
                    {
                        try
                        {
                            meta = availableFunctions.First(x => x.Name == fb.FunctionName && x.ArgTypes[0] == val.Type.ToString());
                        }
                        catch (Exception)
                        {
                            Interpreter.Error($"THIS FUNCTION DOESN'T HAVE AN OVERRIDE AVAILABLE FOR THE DATA TYPE '{meta.ArgTypes[0]}' FOR {fb.FunctionName}", val.ToString());
                        }
                    }

                    if (val.Type.ToString() != meta.ArgTypes[0])
                        Interpreter.Error($"WRONG PARAMETER DATA TYPE OF {meta.ArgTypes[0]}, EXCEPTED {val.Type} FOR {fb.FunctionName}", val.ToString());

                    if (!(meta.ArgValues is null))
                    { 
                        var k = meta.ArgValues.Keys.ToArray();

                        meta.ArgValues[k[0]] = val;
                    }

                    var _k = Interpreter.function.FirstOrDefault(z => z.Key.Name == fb.FunctionName).Key;
                    var _v = Interpreter.function.FirstOrDefault(z => z.Key.Name == fb.FunctionName).Value;
                    Interpreter.function.Remove(_k);
                    Interpreter.function.Add(_k, _v);

                    return new Value { OTHER = $"FN-{fb.FunctionName}" };
                }
                else
                {
                    var k = meta.ArgValues.Keys.ToArray();
                    Value val;

                    for (int i = 0; i < fb.Args.Length; i++)
                    {
                        val = new Value(vals.ContainsKey(fb.Args[i]) ? vals[fb.Args[i]].ToString() : fb.Args[i], true);

                        if (meta.ArgTypes[i] != val.Type.ToString())
                            Interpreter.Error($"WRONG PARAMETER DATA TYPE USED FOR {k[i]}, EXPECTED {meta.ArgTypes[i]}", fb.Args[i]);

                        meta.ArgValues[k[i]] = val;
                    }

                    var _k = Interpreter.function.FirstOrDefault(z => z.Key.Name == fb.FunctionName).Key;
                    var _v = Interpreter.function.FirstOrDefault(z => z.Key.Name == fb.FunctionName).Value;
                    Interpreter.function.Remove(_k);
                    Interpreter.function.Add(_k, _v);

                    return new Value { OTHER=$"FN-{fb.FunctionName}" };
                }
            }

            if (isNotOperator)
            {

                Value eval = FuncExtensions(fb.FunctionName, args);

                if (eval.Type != ValueType.BOOLEAN)
                    Interpreter.Error($"Cannot use the NOT wrapper with {eval.Type} from function: {fb.FunctionName}", fb.FunctionName);

                eval.BOOLEAN = !eval.BOOLEAN;
                return eval;
            }

            return FuncExtensions(fb.FunctionName, args);
        }

        private static Value CondEval(IBlock block, bool isNotOperator, Dictionary<string, Value> vals)
        {
            ConditionBlock cb = block as ConditionBlock;

            // First check if any variables here are stored
            if (cb.Left.Type == ValueType.STRING)
                cb.Left = (vals.ContainsKey(cb.Left.STRING)) ? vals[cb.Left.STRING] : cb.Left;

            if (cb.Right.Type == ValueType.STRING)
                cb.Right = (vals.ContainsKey(cb.Right.STRING)) ? vals[cb.Right.STRING] : cb.Right;

            if (cb.Left.Type == ValueType.CHARACTER)
                cb.Left = (vals.ContainsKey(cb.Left.CHARACTER.ToString())) ? vals[cb.Left.CHARACTER.ToString()] : cb.Left;

            if (cb.Right.Type == ValueType.CHARACTER)
                cb.Right = (vals.ContainsKey(cb.Right.CHARACTER.ToString())) ? vals[cb.Right.CHARACTER.ToString()] : cb.Right;

            // Good, now we check if its an integer, we raise it to the next highest type (Float/Double -> REAL)
            if (cb.Left.Type == ValueType.INTEGER)
                cb.Left = cb.Left.Convert(ValueType.REAL);

            if (cb.Right.Type == ValueType.INTEGER)
                cb.Right = cb.Right.Convert(ValueType.REAL);

            if (isNotOperator)
            {
                Value eval = DoExpr(cb.Left, cb.CompareOp, cb.Right);

                if (eval.Type != ValueType.BOOLEAN)
                    Interpreter.Error($"Cannot use the NOT wrapper with {eval.Type}", eval.ToString());

                eval.BOOLEAN = !eval.BOOLEAN;
                return eval;
            }

            return DoExpr(cb.Left, cb.CompareOp, cb.Right);
        }
        #endregion

        #region Block Function Evaluations

        private static FuncMetaData[] availableFunctions = new FuncMetaData[]
        {
            // STRING PSEUDOCODE FUNCTIONS
            new FuncMetaData { Name = "Title", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },
            new FuncMetaData { Name = "Trim", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },
            new FuncMetaData { Name = "Upper", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },
            new FuncMetaData { Name = "Lower", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },

            // CONVERSION PSEUDOCODE FUNCTIONS (with overrides)
            new FuncMetaData { Name = "INT", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },
            new FuncMetaData { Name = "INT", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "REAL" } },
            new FuncMetaData { Name = "INT", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "BOOLEAN" } },

            new FuncMetaData { Name = "REAL", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "STRING" } },
            new FuncMetaData { Name = "REAL", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "INTEGER" } },
            new FuncMetaData { Name = "REAL", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[] { "BOOLEAN" } },

            // DATE PSEUDOCODE FUNCTIONS
            new FuncMetaData { Name = "DAY", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "MONTH", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "YEAR", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "HOURS", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "MINUTES", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "SECONDS", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]},
            new FuncMetaData { Name = "MILISECONDS", type = FuncMetaData.Type.FUNCTION, ArgTypes = new string[0]}
        };
        private static bool isPreDefined(string subject) => availableFunctions.Any(e => e.Name == subject);
        
        private static Value FuncExtensions(string function, string expression)
        {
            // Deal with Pseudo/Pre-Defined Function(s) first
            if(isPreDefined(function))
            {
                var result = PreDefinedFunctions(function, expression);

                if(ReferenceEquals(result, Value.Zero))
                {
                    Interpreter.Error($"Problem identifying the follow pseudo function: {function}\nHere are the list of them: {string.Join(", ", availableFunctions)}", function);
                }

                return result;
            }

            return new Value();
        }

        private static Value PreDefinedFunctions(string fun, string ex, Dictionary<string,Value> val = null)
        {
            Value result = Value.Zero;

            // Remove the function to prevent StackOverflowException (If any)

            if (ex.Contains('(') || ex.Contains(')'))
            {
                int funcStart = ex.IndexOf('(') + 1;
                int funcEnd = ex.LastIndexOf(')') - funcStart;
                ex = ex.Substring(funcStart, funcEnd);
            }

            #region String functions
            if (fun == "Lower" || fun == "Upper" || fun == "Trim")
            {
                result = PerformExpression(val, ex);

                switch (fun)
                {
                    case "Lower":
                        result.STRING = result.STRING.ToLower();
                        break;

                    case "Upper":
                        result.STRING = result.STRING.ToUpper();
                        break;

                    case "Trim":
                        result.STRING = result.STRING.Trim();
                        break;
                }
            }
            else if (fun == "Title")
            {
                var text = ex.ToCharArray();
                // We force the first char to be Title automatically
                text[0] = char.ToUpper(text[0]);

                // If the character before the current character is whitespace, set the current char to upper
                for (int i = 1; i < text.Length; i++)
                    if (char.IsWhiteSpace(text[i - 1]))
                        text[i] = char.ToUpper(text[i]);

                result = new Value(String.Join("", text));
            }

            if(!result.Equals(Value.Zero))
                return result;
            #endregion

            #region Date functions
            if(new List<string> { "DAY", "MONTH", "YEAR", "HOURS", "MINUTES", "SECONDS", "MILISECONDS" }.Contains(fun))
            {
                var d = DateTime.Now;

                switch (fun)
                {
                    case "DAY":
                        result = new Value(d.Day);
                        break;

                    case "MONTH":
                        result = new Value(d.Month);
                        break;

                    case "YEAR":
                        result = new Value(d.Year);
                        break;

                    case "HOURS":
                        result = new Value(d.Hour);
                        break;

                    case "MINUTES":
                        result = new Value(d.Minute);
                        break;

                    case "SECONDS":
                        result = new Value(d.Second);
                        break;

                    case "MILISECONDS":
                        result = new Value(d.Millisecond);
                        break;
                }
                if (!result.Equals(Value.Zero))
                    return result;
            }
            #endregion

            #region Type conversions
            if(fun == "INT")
            {
                Value express = PerformExpression(val, ex);

                if(!availableFunctions.Where(y => y.Name == "INT").Any(x => x.ArgTypes.Contains(express.Type.ToString())))
                {
                    Interpreter.Error("INT CANNOT SUPPORT THE GIVEN INPUT TO CONVERT TO INT", ex);
                }

                switch (express.Type)
                {
                    case ValueType.REAL:
                        express = new Value(Convert.ToInt32(express.REAL));
                        break;
                    case ValueType.STRING:
                        if(Int32.TryParse(express.ToString(), out int i))
                        {
                            express = new Value(i);
                        }
                        else
                        {
                            Interpreter.Error("INT CANNOT SUPPORT THE GIVEN STRING INPUT", ex);
                        }
                        break;
                    case ValueType.BOOLEAN:
                        express = new Value((express.BOOLEAN)?true:false);
                        break;
                    default:
                        break;
                }

                return express;
            }

            if (fun == "REAL")
            {
                Value express = PerformExpression(val, ex);

                // Fix the expression if it results back to REAL
                if (express.Type == ValueType.REAL && express.REAL % 1 == 0)
                    express = express.Convert(ValueType.INTEGER);

                if (!availableFunctions.Where(y => y.Name == "REAL").Any(x => x.ArgTypes.Contains(express.Type.ToString())))
                {
                    Interpreter.Error("REAL CANNOT SUPPORT THE GIVEN INPUT TO CONVERT TO REAL", ex);
                }

                switch (express.Type)
                {
                    case ValueType.INTEGER:
                        express = new Value(Convert.ToDouble(express.INT));
                        break;
                    case ValueType.STRING:
                        if (Int32.TryParse(express.ToString(), out int i))
                        {
                            express = new Value(i);
                        }
                        else
                        {
                            Interpreter.Error("REAL CANNOT SUPPORT THE GIVEN STRING INPUT", ex);
                        }
                        break;
                    case ValueType.BOOLEAN:
                        express = new Value((express.BOOLEAN) ? 1.0 : 0.0);
                        break;
                    default:
                        break;
                }

                return express;
            }
            #endregion

            return result;
        }

        #endregion

        #endregion Expression Operations
    }
}