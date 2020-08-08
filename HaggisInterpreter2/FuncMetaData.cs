using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaggisInterpreter2
{
    public struct FuncMetaData
    {
        public enum Type
        {
            FUNCTION,
            PROCEDURE
        }

        /// <summary>
        /// Functions Name
        /// </summary>
        public string Name;

        /// <summary>
        /// Functions Parameters Types (In order)
        /// </summary>
        public string[] ArgTypes;

        public Dictionary<string, Value> ArgValues;

        /// <summary>
        /// Functions type: Is it a function or a parameter?
        /// </summary>
        public Type type;

        public ValueType returnType;

        /// <summary>
        /// Holds the last number where the function or procedure terminates
        /// </summary>
        public int FunctionEnd;
    }
}
