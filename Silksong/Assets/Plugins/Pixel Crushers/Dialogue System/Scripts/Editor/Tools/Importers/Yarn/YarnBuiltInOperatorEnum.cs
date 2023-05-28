#if USE_YARN

namespace PixelCrushers.DialogueSystem.Yarn
{
        // public static readonly List<string> BuiltInFunctions = new List<string>()
        public enum YarnBuiltInOperator
        {
            // Logical Operators
            // "EqualTo",  // Equality: eq or is or ==
            // "neq",      // Inequality: neq or !
            // ">",        // Greater than: gt or >
            // "<",        // Less than: lt or <
            // "<=",       // Less than or equal to: lte or <=
            // ">=",       // Greater than or equal to: gte or >=
            // "Or",       // Boolean OR: or or ||
            // "Xor",      // Boolean XOR: xor or ^
            // "Not",      // Boolean Negation: !
            // "And",      // Boolean AND: and or &&

            // // Math operators
            // "+",    // Addition: +
            // "-",    // Subtraction: -
            // "*",    // Multiplication: *
            // "/",    // Division: /
            // "%",    // Truncating Remainder Division: %
            // "(",    // Brackets: ( to open the brackets and ) to close them
            // ")",    // Brackets: ( to open the brackets and ) to close them

            // From Yarn.TokenType internal enum (Compiler.cs)
            EqualTo, // ==, eq, is
            GreaterThan, // >, gt
            GreaterThanOrEqualTo, // >=, gte
            LessThan, // <, lt
            LessThanOrEqualTo, // <=, lte
            NotEqualTo, // !=, neq

            // Logical operators
            Or, // ||, or
            And, // &&, and
            Xor, // ^, xor
            Not, // !, not

            // this guy's special because '=' can mean either 'equal to'
            // or 'becomes' depending on context
            EqualToOrAssign, // =, to

            UnaryMinus, // -; this is differentiated from Minus
                        // when parsing expressions

            Add, // +
            Minus, // -
            Multiply, // *
            Divide, // /
            Modulo, // %

            AddAssign, // +=
            MinusAssign, // -=
            MultiplyAssign, // *=
            DivideAssign, // /=
        };

    public static class YarnBuiltInOperatorExt
    {
        // Purposely wrapping the ToString() call in case I need to massage the results.
        // Most likely don't need to, though
        public static string Name(this YarnBuiltInOperator op)
        {
            return op.ToString();
        }

        public static bool Matches(this YarnBuiltInOperator op, string opString)
        {
            return op.Name() == opString;
        }

        public static bool IsUnary(this YarnBuiltInOperator op)
        {
            return op == YarnBuiltInOperator.Not || op == YarnBuiltInOperator.UnaryMinus;
        }

        public static bool IsBinary(this YarnBuiltInOperator op)
        {
            return !op.IsUnary();
        }

        public static string LuaOperator(this YarnBuiltInOperator op)
        {
            // EqualTo, // ==, eq, is
            // GreaterThan, // >, gt
            // GreaterThanOrEqualTo, // >=, gte
            // LessThan, // <, lt
            // LessThanOrEqualTo, // <=, lte
            // NotEqualTo, // !=, neq

            // // Logical operators
            // Or, // ||, or
            // And, // &&, and
            // Xor, // ^, xor
            // Not, // !, not

            // // this guy's special because '=' can mean either 'equal to'
            // // or 'becomes' depending on context
            // EqualToOrAssign, // =, to

            // UnaryMinus, // -; this is differentiated from Minus when specified as a CallFunc

            // Add, // +
            // Minus, // -
            // Multiply, // *
            // Divide, // /
            // Modulo, // %

            // AddAssign, // +=
            // MinusAssign, // -=
            // MultiplyAssign, // *=
            // DivideAssign, // /=

            switch (op)
            {
                // Arithmetic
                case YarnBuiltInOperator.Add:
                    return "+";
                case YarnBuiltInOperator.UnaryMinus:
                case YarnBuiltInOperator.Minus:
                    return "-";
                case YarnBuiltInOperator.Multiply:
                    return "*";
                case YarnBuiltInOperator.Divide:
                    return "/";
                // I believe this requires Lua 5.1+, verify which one is used in DialogueSystem
                case YarnBuiltInOperator.Modulo:
                    return "%";

                // UNSUPPORTED: Arithmetic Assignment
                case YarnBuiltInOperator.AddAssign:
                    // return "+";
                    return "<ADD ASSIGN (+=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case YarnBuiltInOperator.MinusAssign:
                    // return "-";
                    return "<SUBTRACT ASSIGN (-=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case YarnBuiltInOperator.MultiplyAssign:
                    // return "*";
                    return "<MULTIPLY ASSIGN (*=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case YarnBuiltInOperator.DivideAssign:
                    // return "/";
                    return "<DIVIDE ASSIGN (/=) OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                // I believe this requires Lua 5.1+, verify which one is used in DialogueSystem
                // case BuiltInOperatorType.Modulo:
                //     return "%";

                // NOTE: I have no idea why this is called EqualToOrAssign, which worries me.
                //       Shouldn't it just be called the Assign operator? I have to be missing something.
                case YarnBuiltInOperator.EqualToOrAssign:
                    // return "+";
                    return "=";

                // Logical Operators
            // Or, // ||, or
            // And, // &&, and
            // Xor, // ^, xor
            // Not, // !, not
                case YarnBuiltInOperator.Or:
                    return "or";
                case YarnBuiltInOperator.And:
                    return "and";
                case YarnBuiltInOperator.Xor:
                    return "<LOGICAL XOR OPERATOR NOT SUPPORTED, MODIFY YOUR CODE'S LOGIC>";
                case YarnBuiltInOperator.Not:
                    return "not";

                // Relational Operators
                case YarnBuiltInOperator.EqualTo:
                    return "==";
                case YarnBuiltInOperator.GreaterThan:
                    return ">";
                case YarnBuiltInOperator.GreaterThanOrEqualTo:
                    return ">=";
                case YarnBuiltInOperator.LessThan:
                    return "<";
                case YarnBuiltInOperator.LessThanOrEqualTo:
                    return "<=";
                case YarnBuiltInOperator.NotEqualTo:
                    return "!=";

            }

            return null;
        }

        public static string LuaOperatorType(this YarnBuiltInOperator op)
        {
            return $"YarnOperatorType.{op}";
        }
    }
}

#endif