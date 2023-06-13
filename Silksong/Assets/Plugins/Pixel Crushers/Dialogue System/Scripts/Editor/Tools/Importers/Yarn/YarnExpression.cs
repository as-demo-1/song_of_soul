#if USE_YARN

using System;
using System.Collections.Generic;

using UnityEngine.Assertions;

using Yarn;

namespace PixelCrushers.DialogueSystem.Yarn
{
    // public static readonly List<string> BuiltInFunctions = new List<string>()
    public enum YarnExpressionElementType
    {
        CallFunc = 1,
        // Expression,
        Operand,
        Operation,
    }

    public static class YarnExpressionElementTypeExt
    {
        public static bool IsCallFunc(this YarnExpressionElementType el)
        {
            return el == YarnExpressionElementType.CallFunc;
        }

        public static bool IsOperand(this YarnExpressionElementType el)
        {
            return el == YarnExpressionElementType.Operand;
        }

        public static bool IsOperation(this YarnExpressionElementType el)
        {
            return el == YarnExpressionElementType.Operation;
        }
    }

    public class YarnExpressionElement
    {
        private Operand _operand = null;
        private YarnBuiltInOperator _operator = YarnBuiltInOperator.Add;
        private string _callFunc = null;
        // This list of operands is used by CallFunc and Operator expressions, not Operand expressions
        private List<YarnExpressionElement> _operands = new List<YarnExpressionElement>();

        public YarnExpressionElementType Type { get; private set; }
        public bool EvaluatesToString { get { return GetEvaluatesToString(); } }
        public string CallFunc { get { return _callFunc; } }
        public Operand Operand { get { return _operand; } }
        public YarnBuiltInOperator Operator { get { return _operator; } }
        public IReadOnlyList<YarnExpressionElement> Operands { get { return _operands; } }

        public YarnExpressionElement(YarnBuiltInOperator op, YarnExpressionElement arg1, YarnExpressionElement arg2=null)
        {
            Type = YarnExpressionElementType.Operation;
            _operator = op;
            _operands.Add(arg1);
            if (arg2 != null)
            {
                _operands.Add(arg2);
            }
        }

        public YarnExpressionElement(Operand operand)
        {
            Type = YarnExpressionElementType.Operand;
            _operand = operand;
        }

        public YarnExpressionElement(string callFunc, params YarnExpressionElement[] operands)
        {
            Type = YarnExpressionElementType.CallFunc;
            _callFunc = callFunc;

            foreach (var operand in operands)
            {
                Assert.AreEqual(YarnExpressionElementType.Operation, operand.Type);
                _operands.Add(operand);
            }
        }

        private bool GetEvaluatesToString()
        {
            return GetEvaluatesToString(this);
        }

        private bool GetEvaluatesToString(YarnExpressionElement exp)
        {
            // Any string in any expression element results in the entire thing resolving
            // to a string. That's because the only way other operand types can be mixed with
            // a string is through string concatenation, and the entire expression will resolve
            // to a string value.

            // Base case
            if (exp.Type.IsOperand())
            {
                return exp.Operand.ValueCase.IsStringValue();
            }

            // If we're dealing with an Operation or a CallFunc,
            // evaluate all Operators (or at least until one returns true).
            foreach (var op in exp.Operands)
            {
                if (GetEvaluatesToString(op))
                {
                    return true;
                }
            }

            // If we get down here, there are no string operands anywhere in the expression.
            return false;
        }
    }
}

#endif