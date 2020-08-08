using System;

namespace HaggisInterpreter2
{
    public enum ValueType
    {
        REAL,
        STRING,
        INTEGER,
        BOOLEAN,
        CHARACTER
    }

    public struct Value
    {
        #region Meta Data

        public static readonly Value Zero = new Value(0);
        public ValueType Type { get; set; }

        #endregion Meta Data

        #region Data types

        public double REAL { get; set; }
        public int INT { get; set; }
        public bool BOOLEAN { get; set; }
        public string STRING { get; set; }
        public char CHARACTER { get; set; }
        public string OTHER { get; set; }

        #endregion Data types

        #region Constructors

        public Value(Value val) : this()
        {
            this = val;
        }

        public Value(double val) : this()
        {
            this.Type = ValueType.REAL;
            this.REAL = val;
        }

        public Value(string val, bool evalType = false) : this()
        {
            if (!evalType)
            {
                this.Type = ValueType.STRING;
                this.STRING = val;
            }
            else
            {
                if (val.ToLower() == "false" || val.ToLower() == "true")
                {
                    this.Type = ValueType.BOOLEAN;
                    this.BOOLEAN = val.ToLower() != "false";
                }
                else if (Int32.TryParse(val, out int intVal))
                {
                    this.Type = ValueType.INTEGER;
                    this.INT = intVal;
                }
                else if (double.TryParse(val, out double doubleVal))
                {
                    this.Type = ValueType.REAL;
                    this.REAL = doubleVal;
                }
                else
                {
                    if (val.Length == 1)
                    {
                        this.Type = ValueType.CHARACTER;
                        this.CHARACTER = val[0];
                    }
                    else
                    {
                        this.Type = ValueType.STRING;
                        this.STRING = val;
                    }
                }
            }
        }

        public Value(int val) : this()
        {
            this.Type = ValueType.INTEGER;
            this.INT = val;
        }

        public Value(bool val) : this()
        {
            this.Type = ValueType.BOOLEAN;
            this.BOOLEAN = val;
        }

        public Value(char val) : this()
        {
            this.Type = ValueType.CHARACTER;
            this.CHARACTER = val;
        }

        #endregion Constructors

        public Value Convert(ValueType type)
        {
            //TODO: Work on this functionality
            if (this.Type != type)
            {
                string target;
                switch (type)
                {
                    case ValueType.REAL:
                        double d;
                        target = !(STRING is null) ? STRING : INT.ToString();
                        // try to parse as double, if failed read value as string
                        if (double.TryParse(target, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                        {
                            this = new Value(d);
                        }
                        else
                        {
                            throw new Exception($"ERROR: Failed to attempt to convert {this} as REAL");
                        }
                        break;

                    case ValueType.INTEGER:
                        // Change INT to REAL
                        int i;
                        target = !(STRING is null) ? STRING : REAL.ToString();

                        if (Int32.TryParse(target, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                        {
                            this = new Value(i);
                        }
                        else
                        {
                            throw new Exception($"ERROR: Failed to attempt to convert {this} as INT");
                        }
                        break;

                    case ValueType.STRING:
                        this = new Value(REAL.ToString());
                        break;
                }
            }
            return this;
        }

        public override string ToString()
        {
            if (this.Type == ValueType.REAL)
                return this.REAL.ToString();

            if (this.Type == ValueType.INTEGER)
                return this.INT.ToString();

            if (this.Type == ValueType.BOOLEAN)
                return this.BOOLEAN == true ? "TRUE" : "FALSE";

            if (this.Type == ValueType.CHARACTER)
                return this.CHARACTER.ToString();

            return this.STRING;
        }
    }
}