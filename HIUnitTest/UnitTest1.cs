using Microsoft.VisualStudio.TestTools.UnitTesting;
using HaggisInterpreter2;
using System;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class InterpreterTests
    {
        [TestMethod]
        public void TestDeclear()
        {

            var he = new HaggisInterpreter2.Interpreter(new string[] { "DECLEAR item AS INTEGER", 
                "DECLEAR item2 AS REAL INITIALLY 25.5",
                "DECLEAR item3 AS CHARACTER INITIALLY 'A'",
                "DECLEAR item4 AS BOOLEAN",
                "DECLEAR item5 AS STRING INITIALLY \"Hey! Thats pretty cool!\""}, new Interpreter.FLAGS());

            he.Execute();

            bool[] tests = new bool[]
            {
                he.variables.Any(h => h.Key == "item" && h.Value == new Value(0)),
                he.variables.Any(h => h.Key == "item2" && h.Value == new Value(25.5)),
                he.variables.Any(h => h.Key == "item3" && h.Value == new Value('A')),
                he.variables.Any(h => h.Key == "item4" && h.Value == new Value(false)),
                he.variables.Any(h => h.Key == "item5" && h.Value == new Value("Hey! Thats pretty cool!"))
            };

            Assert.IsTrue(tests.Any(test => test == false));

        }

        [TestMethod()]
        public void TestSet()
        {
            var he = new HaggisInterpreter2.Interpreter(new string[] { "SET item TO 123",
                "SET item1 TO REAL(314)",
                "SET item2 TO (5 * 2) + 50", // Equals '60'
                "SET item4 TO INT(3.142)",
                "SET item5 TO Lower(\"HEY!\")"}, new Interpreter.FLAGS());

            he.Execute();

            bool[] tests = new bool[]
            {
                he.variables.Any(h => h.Key == "item" && h.Value == new Value(123)),
                he.variables.Any(h => h.Key == "item2" && h.Value == new Value(314d)),
                he.variables.Any(h => h.Key == "item3" && h.Value == new Value(60)),
                he.variables.Any(h => h.Key == "item4" && h.Value == new Value(3)),
                he.variables.Any(h => h.Key == "item5" && h.Value == new Value("hey!"))
            };

            Assert.IsTrue(tests.Any(test => test == false));
        }
    }
}
