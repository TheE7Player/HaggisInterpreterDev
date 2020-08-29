using Microsoft.VisualStudio.TestTools.UnitTesting;
using HaggisInterpreter2;
using System;
using System.Linq;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class InterpreterTests
    {
        [TestMethod]
        public void TestDeclear()
        {

            var he = new Interpreter(new string[] { "DECLEAR item AS INTEGER", 
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
            var he = new Interpreter(new string[] { "SET item TO 123",
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

        [TestMethod()]
        public void TestRecieve()
        {
            var he = new Interpreter(new string[] { "#<DEBUG: [input]-25>",
                "RECEIVE input FROM (INTEGER) KEYBOARD",
                "SEND \"Expression is: \" & 5*10 TO DISPLAY",
                "SEND \"Number is: \" & input TO DISPLAY"}, new Interpreter.FLAGS { Inputs = new Dictionary<string, string> { {"input", "25" } } } );

            var sw = new System.IO.StringWriter();

            Console.SetOut(sw);
            he.Execute();
       
            Assert.IsTrue(sw.ToString() == "Expression is: 50\r\nNumber is: 25\r\n");
        }

        [TestMethod()]
        public void TestIfNormal()
        {
            var he = new Interpreter(new string[] { "#<DEBUG: [input]-25>",
                "RECEIVE input FROM (INTEGER) KEYBOARD",
                "IF input > 20 THEN SEND \"Input is more than 20\" TO DISPLAY END IF",
                "IF input < 50 THEN SEND \"Input is less than 20\" TO DISPLAY END IF",

                "IF input > 0 AND input < 50 THEN",
                    "SEND \"input is between 0 to 50\" TO DISPLAY",
                "ELSE",
                    "SEND \"input isn't between 0 to 50\" TO DISPLAY",
                "END IF",

                "IF input > 50 AND input < 100 THEN",
                    "SEND \"input is between 50 to 100\" TO DISPLAY",
                "ELSE",
                    "SEND \"input isn't between 50 to 100\" TO DISPLAY",
                "END IF" },
                new Interpreter.FLAGS { Inputs = new Dictionary<string, string> { { "input", "25" } } });

            var sw = new System.IO.StringWriter();

            Console.SetOut(sw);
            he.Execute();

            Assert.IsTrue(sw.ToString() == "Input is more than 20\r\nInput is less than 20\r\ninput is between 0 to 50\r\ninput isn't between 50 to 100\r\n");
        }
    }
}
