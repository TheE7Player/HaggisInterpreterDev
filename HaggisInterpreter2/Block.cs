using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaggisInterpreter2
{
    /// <summary>
    /// An Expression Simplifier ~ Used to make expressions more robust (Logic will be simplified)
    /// </summary>
    /// 
    public enum BlockType
    {

        Null, Variable, Literal,

        Expression, BinOp, IterWhile, IterRepeat,

        Text, Function,

        Record, Class
    }

    public interface IBlock
    {
        /// <summary>
        /// Holds the output of the block (If any)
        /// </summary>
        public Value Value { get;  set; }

        /// <summary>
        /// Importance level - Used if doing calcuations with brackets (BOMDAS/PEMDAS)
        /// (0 is the lowest level available)
        /// </summary>
        public int OrderLevel { get; set; }

        /// <summary>
        /// Helps to blocks to preserve the order they are when they get sorted
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// If the block carries an operation from the right hand side (If any)
        /// </summary>
        public string BinaryOp { get; set; }
        /// <summary>
        /// The Blocks type
        /// </summary>
        public BlockType blockType { get; set; }

        /// <summary>
        /// If the given block has any left and right operands. True if both are not empty.
        /// </summary>
        /// <returns>True if has left and right, False if either or no valves are inserted</returns>
        //public bool HasLeftRightOperand();
       
    }

    #region Blocks

    /// <summary>
    /// Block ~ A single item or object
    /// </summary>
    public class Block : IBlock
    {
        public BlockType blockType { get; set; }

        public int OrderLevel { get; set; }

        public Value Value { get; set; }

        public string BinaryOp { get; set; }

        public int OrderNumber { get; set; }
    }

    /// <summary>
    /// Conditional Block ~ Holds an expression that will be evaluated TRUE or FALSE
    /// </summary>
    public class ConditionBlock : IBlock
    {

        public BlockType blockType { get; set; }
        public string BinaryOp { get; set; }

        public string CompareOp { get; set; }

        public Value Value { get; set; }

        /// <summary>
        /// Left side of the comparsion (Comparing)
        /// </summary>
        public Value Left { get; set; }

        /// <summary>
        /// Right side of the comparsion (Comparing To)
        /// </summary>
        public Value Right { get; set; }
        public int OrderLevel { get; set; }
        public int OrderNumber { get; set; }
    }

    /// <summary>
    /// Function Block ~ Holds Metadata on PROCEDURE and FUNCTION
    /// </summary>
    public class FuncBlock : IBlock
    {
        public BlockType blockType { get; set; }
        public string BinaryOp { get; set; }
        public Value Value { get; set; }
        public string[] Args { get; set; }
        public string FunctionName { get; set; }
        public int OrderLevel { get; set; }
        public int OrderNumber { get; set; }

    }

    
    /// <summary>
    /// Statement Block ~ Holds the metadata for IF statements
    /// </summary>
    public class StatementBlock : IBlock
    {
        public BlockType blockType { get; set; }

        public int OrderLevel { get; set; }

        public Value Value { get; set; }

        public string BinaryOp { get; set; }

        public int OrderNumber { get; set; }
        // END OF CONTRACT
        
        public string Expression { get; set; }

        public Dictionary<int, string> OnFalse { get; set; }
        public Dictionary<int, string> OnTrue { get; set; }

        public int CondStart { get; set; }
        public int CondEnd { get; set; }

        public StatementBlock Copy()
        {
            StatementBlock sb = new StatementBlock();
            sb.blockType = this.blockType;
            sb.OrderLevel = this.OrderLevel;
            sb.Value = this.Value;
            sb.BinaryOp = this.BinaryOp;
            sb.OrderNumber = this.OrderNumber;
            sb.Expression = this.Expression;
            sb.OnFalse = this.OnFalse;
            sb.OnTrue = this.OnTrue;

            sb.CondStart = this.CondStart;
            sb.CondEnd = this.CondEnd;

            return sb;
        }

    }

    #endregion

    public static class BlockParser
    {
        private static int FindLastChar(string[] ch, string target)
        {
            for (int i = ch.Length - 1; i >= 0; i--)
            {
                if (ch[i] == target)
                    return i;
            }
            return -1;
        }
        private static bool CanLookAhead(int current, int len) => ((current + 1) < len); //=> ((current + 1) < len) ? true : false;
        public static List<IBlock> GenerateBlocks(string[] expr, Dictionary<string, Value> vals)
        {          
            // Holds the current order level (used for calculations or expressions)
            int orderLevel = 0;

            if(expr.Length == 1)
            {
                var s_list = new List<IBlock>(1);
                if (vals.ContainsKey(expr[0]))
                {
                    s_list.Add(new Block { BinaryOp = null, blockType = BlockType.Variable, Value = vals[expr[0]], OrderLevel = orderLevel, OrderNumber = s_list.Count});
                    return s_list;
                }

                s_list.Add(new Block { BinaryOp = null, blockType = BlockType.Literal, Value = new Value(expr[0], true), OrderLevel = orderLevel, OrderNumber = s_list.Count });
                return s_list;
            }

            var _list = new List<IBlock>(2);
            int max_len = expr.Length;

            //string express = String.Join(" ", expr);

            for (int i = 0; i < max_len; i++)
            {
                // Look ahead for a function block?
                if (CanLookAhead(i, max_len))
                {
                    if (expr[i + 1] == "(")
                    {
                        var func_name = expr[i];
                        i++;
                        // Check if the function carries over any params
                        if (CanLookAhead(i + 1, max_len))
                        {
                            if (expr[i + 1] == ")")
                            {
                                i += 2;
                                _list.Add(new FuncBlock { FunctionName = func_name, Value = Value.Zero, Args = null, OrderLevel = orderLevel, blockType = BlockType.Function, OrderNumber = _list.Count });
                                orderLevel++;
                                continue;
                            }

                            i++;
                            List<string> args = new List<string>();
                            string parameters = string.Join(" ", expr);
                            int paramStart = parameters.IndexOf('(')+1;
                            int paramEnd = parameters.LastIndexOf(')');
                            parameters = parameters.Substring(paramStart, paramEnd - paramStart).Trim();

                            var new_arr = Expression.Evaluate(parameters);
                            char[] fix;
                            var new_expr = expr.ToList();
                            int currIndex = 0;
                            for (int z = 0; z < new_arr.Length; z++)
                            {
                                currIndex = new_expr.IndexOf(new_arr[z]);
                                fix = new_arr[z].ToCharArray();
                                if (fix[fix.Length - 1] == ',' && fix.Length>1)  
                                { 
                                    Array.Resize(ref fix, fix.Length - 1);
                                    new_expr[currIndex] = new string(fix);
                                    new_expr.Insert(currIndex + 1, ",");    
                                }
                            }

                            new_arr = null;
                            parameters = null;
                            fix = null;

                            expr = new_expr.ToArray();
                            max_len = expr.Length;
                            new_expr = null;

                            var arg_sb = new StringBuilder();
                            int last_bracket = FindLastChar(expr, ")");
                            try
                            {
                                if (expr[last_bracket] != ")")
                                    throw new Exception();
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("MISSING OR WRONG BALANCE OF BRACKETS FOR EXPRESSION FOR ARGUMENT", expr[i]);
                            }
                            

                            while (i < max_len)
                            {
                                if (expr[i] == ")")
                                {
                                    if (!(i == expr.Length - 1 || Expression.validOperations.Contains((expr[i + 1][0]))))
                                    { continue; }

                                    if (i != last_bracket)
                                    { i++; continue; }

                                    if (arg_sb.Length > 0)
                                    {
                                        args.Add(arg_sb.ToString().Trim());
                                        arg_sb.Clear();
                                        i++;
                                    }
                                    break;
                                }
                                else
                                {
                                     i++;
                                }

                                if (arg_sb.Length == 0)
                                    arg_sb.Append(expr[i - 1]);
                                else
                                {
                                    if(double.TryParse(expr[i - 1], out double _))
                                        arg_sb.Append(expr[i - 1]);
                                    else
                                        arg_sb.Append($" {expr[i - 1]}");
                                }

                                if (expr[i] == "," || i == expr.Length - 1)
                                {
                                    args.Add(arg_sb.ToString().Trim());
                                    arg_sb.Clear();
                                    i++; continue;
                                }
                            }

                            //TODO: Why add 2 here?!
                            //i += 2;
                            _list.Add(new FuncBlock { FunctionName = func_name, Value = Value.Zero, Args = args.ToArray(), OrderLevel = orderLevel, blockType = BlockType.Function, OrderNumber = _list.Count });
                            orderLevel++;
                            i--;
                            continue;
                        }
                        else
                        {
                            if(expr[i + 1] == ")")
                            {
                                _list.Add(new FuncBlock { FunctionName = func_name, Value = Value.Zero, Args = null, OrderLevel = orderLevel, blockType = BlockType.Function, OrderNumber = _list.Count });
                            }
                        }
                    }
                }

                // The second expression prevents ')' from being excluded if it was a string ('&') concat...
                if (expr[i] == "(" || (expr[i] == ")" && expr[i - 1] != "&"))
                {
                    orderLevel = (expr[i] == "(") ? orderLevel += 1 : orderLevel -= 1;
                    continue;
                }

                // Get the value type
                Value _val = new Value(expr[i], true);

                string op = string.Empty;
                // Get the binary operator (if any)
                if (CanLookAhead(i + 1, max_len))
                {
                    // Prevents issues with code below if the so called index was empty
                    if (!string.IsNullOrEmpty(expr[i + 1]))                                  
                    if (Expression.validOperations.Contains(expr[i + 1][0]))
                    {
                        if (expr[i + 1] != ")")
                        {
                            i++;
                            op = expr[i];

                            // Combine if !=
                            if(CanLookAhead(i + 1, max_len))
                            {
                                string op_concat = $"{op}{expr[i + 1]}";
                                if(op_concat == "!=" || op_concat == "<>") { i++; op = "!="; }

                                // Support for >= and <=
                                if (op_concat == ">=" || op_concat == "<=")
                                { op = op_concat; i++; }
                            }
                        }
                    } 
                }

                // Append the op block itself it there isnt (Could be caused by a function etc)
                if(object.ReferenceEquals(op, string.Empty) && Expression.validOperations.Contains(_val.ToString()[0]) && i != expr.Length - 1)
                {
                    char subject = _val.ToString()[0];
                    if ((subject == '(' || subject == ')') && expr[i - 1] != "&")
                    {
                        _list.Add(new Block { blockType = BlockType.BinOp, BinaryOp = _val.ToString(), OrderLevel = orderLevel, OrderNumber = _list.Count });
                        continue;
                    }
                }

                // Amend the information now
                BlockType bType = BlockType.Literal;

                if (!(vals is null))
                {
                    if (vals.ContainsKey((op != String.Empty) ? expr[i - 1] : expr[i]))
                        bType = BlockType.Variable;
                }

                if ((_val.Type == ValueType.STRING || _val.Type == ValueType.CHARACTER) && bType != BlockType.Variable)
                    bType = BlockType.Text;

                // Check 1
                if((i - 1) > 1)
                if (expr[i].Length == 1 && _val.Type == ValueType.CHARACTER && expr[i - 1] != "&")
                {
                    if (Expression.validOperations.Contains(_val.CHARACTER) && op is null)
                        bType = BlockType.BinOp;
                }

                // Check 2
                if ((expr[i].Length == 1 && _val.ToString().Length == 1) && Expression.validOperations.Contains(_val.ToString()[0]))
                {
                    // Check 3 - Test if '&' op is in play
                    if(op != "&")
                        bType = BlockType.BinOp;
                }
                

                if (Expression.validComparisons.Contains(expr[i]))
                {
                    int safeIndex = (i + 1 < expr.Length - 1) ? i + 1 : i;

                    if(safeIndex == expr.Length - 1)
                    {
                        bType = BlockType.Text;
                    }
                    else
                    try
                    {
                        if (!(_list[_list.Count - 1].GetType().Name == "FuncBlock"))
                        {
                            string expr1 = expr[safeIndex + 1];
                            string expr0 = expr[safeIndex];
                            Value Right = Value.Zero;

                            if (!string.IsNullOrEmpty(expr1))
                                if (!Expression.validComparisons.Contains(expr1) && !Expression.validOperations.Contains(expr1[0]))
                                    Right = new Value(expr1, true);
                                else
                                    Right = new Value(expr0, true);

                            _list.Add(new ConditionBlock { blockType = BlockType.Expression, Value = Value.Zero, CompareOp = op, Left = _val, Right = Right, OrderLevel = orderLevel, OrderNumber = _list.Count });
                            i += 1;
                            if (CanLookAhead(i + 1, max_len))
                            {
                                if (Expression.validComparisons.Contains(expr[i + 1]))
                                {
                                    _list[_list.Count - 1].BinaryOp = expr[i + 1];
                                    i++;
                                    continue;
                                }
                            }
                            i++;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        string expr1 = expr[safeIndex + 1];
                        string expr0 = expr[safeIndex];
                        Value Right = Value.Zero;

                        if(!string.IsNullOrEmpty(expr1))
                        if (!Expression.validComparisons.Contains(expr1) && !Expression.validOperations.Contains(expr1[0]))
                            Right = new Value(expr1, true);
                        else
                            if((safeIndex + 1) == expr.Length - 1)  // [!] Added length for end characters which could be '-' or '+' etc
                                Right = new Value(expr1, true);
                            else
                                Right = new Value(expr0, true);

                            _list.Add(new ConditionBlock { blockType = BlockType.Expression, Value = Value.Zero, CompareOp = op, Left = _val, Right = Right, OrderLevel = orderLevel, OrderNumber = _list.Count });
                        i += 1;
                        if (CanLookAhead(i + 1, max_len))
                        {
                            if (Expression.validComparisons.Contains(expr[i + 1]))
                            {
                                _list[_list.Count - 1].BinaryOp = expr[i + 1];
                                i++;
                                continue;
                            }

                            i++;
                        }
                        continue;
                    }                   
                }
                

                if(bType != BlockType.BinOp)
                    _list.Add(new Block { blockType = bType, Value = _val, BinaryOp = op, OrderLevel = orderLevel, OrderNumber = _list.Count });
                else
                    _list.Add(new Block { blockType = bType, BinaryOp = _val.ToString(), OrderLevel = orderLevel, OrderNumber = _list.Count });
            }

            return _list;
        }

        /// <summary>
        /// Reorders the blocks based on Order Level (Based on BOMDAS/PEMDAS)
        /// </summary>
        /// <param name="blocks">The list of the blocks to sort by</param>
        /// <returns></returns>
        public static Dictionary<int, List<IBlock>> SortByOrder(List<IBlock> blocks)
        {
            SortedDictionary<int, List<IBlock>> sortLevel = new SortedDictionary<int, List<IBlock>>();

            int indexLevel = 0;
            foreach (IBlock b in blocks)
            {
                indexLevel = b.OrderLevel;
                
                if(!sortLevel.ContainsKey(indexLevel))
                {
                    sortLevel.Add(indexLevel, new List<IBlock>(1));
                }

                sortLevel[indexLevel].Add(b);
            }

            // Then we reverse the sorted keys and return it back into a normal dictionary
            return sortLevel.OrderByDescending(kvp => kvp.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public static int GetHighestOrder(List<IBlock> blocks)
        {
            int HighestIndex = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].OrderLevel > HighestIndex)
                    HighestIndex = blocks[i].OrderLevel;
            }
            return HighestIndex;
        }
    }
}