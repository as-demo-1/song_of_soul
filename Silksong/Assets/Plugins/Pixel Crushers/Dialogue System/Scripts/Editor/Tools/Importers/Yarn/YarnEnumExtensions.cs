#if USE_YARN

using System.Collections.Generic;

// NOTE: This file holds class extensions for a few of Yarn's commonly used Enums,
//       e.g. Instruction.Types.OpCode.
//       Because of how verbose their naming can be, it makes code a little less readable.
//       So I've created these Enum extensions in order to make the code a bit more shorter/readable.
//       The syntax isn't bad, but it's not exactly the best way to handle this since it requires function
//       calls instead of property accesses. But I see no easy way to extend these enums to tack on some
//       properties, so this is about the best I can come up with.
namespace Yarn
{
    public static class InstructionOpCodeExtensions
    {
        // NOTE: In many ways, designing this Yarn importer would be easier if we could operate on
        //       the list of tokens in a Yarn text file. But I don't see any way to access that functionality from
        //       the Yarn compiler/parser, so we are stuck trying to deduce exactly what each step of a Yarn
        //       program is doing by scanning through its list of instructions.
        //
        //       That means we take an instruction, then scan sequentially through the instruction list
        //       looking for a matching instruction, and declaring that subset of instructions are particular
        //       type of Yarn operation.
        //
        //       In order to make the code a bit more readable, I have created sets of instructions that signal
        //       the both the start and termination of logical steps.
        //
        //      It may make more sense to stick these in the YarnProject file, closer to where the logic that
        //      uses these is defined. But I made the decision to stick them here, since it makes the syntax
        //      shorter / cleaner / more readable, e.g. opcode.IsStatementTerminatingOpCode().

        private static HashSet<Instruction.Types.OpCode> CommandOpCodes = new HashSet<Instruction.Types.OpCode>
        {
            Instruction.Types.OpCode.CallFunc,
            Instruction.Types.OpCode.RunCommand,
        };

        private static HashSet<Instruction.Types.OpCode> PushOpCodes = new HashSet<Instruction.Types.OpCode>
        {
            Instruction.Types.OpCode.PushBool,
            Instruction.Types.OpCode.PushFloat,
            Instruction.Types.OpCode.PushNull,
            Instruction.Types.OpCode.PushString,
            Instruction.Types.OpCode.PushVariable,
        };

        private static HashSet<Instruction.Types.OpCode> DialogueEntryOpCodes = new HashSet<Instruction.Types.OpCode>
        {
            Instruction.Types.OpCode.AddOption,
            // Instruction.Types.OpCode.Jump,
            // Instruction.Types.OpCode.JumpIfFalse,
            // Instruction.Types.OpCode.JumpTo,
            Instruction.Types.OpCode.RunCommand,
            Instruction.Types.OpCode.RunLine,
            Instruction.Types.OpCode.RunNode,
            // Instruction.Types.OpCode.ShowOptions,
            // Instruction.Types.OpCode.Stop,
            Instruction.Types.OpCode.StoreVariable,
        };

        private static HashSet<Instruction.Types.OpCode> StatementTerminatingOpCodes = new HashSet<Instruction.Types.OpCode>
        {
            Instruction.Types.OpCode.AddOption,
            Instruction.Types.OpCode.Jump,
            Instruction.Types.OpCode.JumpIfFalse,
            Instruction.Types.OpCode.JumpTo,
            Instruction.Types.OpCode.Pop,
            Instruction.Types.OpCode.RunCommand,
            Instruction.Types.OpCode.RunLine,
            Instruction.Types.OpCode.RunNode,
            Instruction.Types.OpCode.ShowOptions,
            Instruction.Types.OpCode.Stop,
            Instruction.Types.OpCode.StoreVariable,
        };

        public static bool IsCommandOpCode(this Instruction.Types.OpCode opcode)
        {
            return CommandOpCodes.Contains(opcode);
        }

        public static bool IsDialogueEntryOpCode(this Instruction.Types.OpCode opcode)
        {
            return DialogueEntryOpCodes.Contains(opcode);
        }

        public static bool IsStatementTerminatingOpCode(this Instruction.Types.OpCode opcode)
        {
            return StatementTerminatingOpCodes.Contains(opcode);
        }

        public static bool IsPushOpCode(this Instruction.Types.OpCode opcode)
        {
            return PushOpCodes.Contains(opcode);
        }

        // Brief syntax predicates for checking value of an OpCode enum
        public static bool IsAddOption(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.AddOption;
        }

        public static bool IsCallFunc(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.CallFunc;
        }

        public static bool IsJump(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.Jump;
        }

        public static bool IsJumpIfFalse(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.JumpIfFalse;
        }

        public static bool IsJumpTo(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.JumpTo;
        }

        public static bool IsPop(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.Pop;
        }

        public static bool IsPushBool(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.PushBool;
        }

        public static bool IsPushFloat(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.PushFloat;
        }

        public static bool IsPushNull(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.PushNull;
        }

        public static bool IsPushString(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.PushString;
        }

        public static bool IsPushVariable(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.PushVariable;
        }

        public static bool IsRunCommand(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.RunCommand;
        }

        public static bool IsRunLine(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.RunLine;
        }

        public static bool IsRunNode(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.RunNode;
        }

        public static bool IsShowOptions(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.ShowOptions;
        }

        public static bool IsStop(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.Stop;
        }

        public static bool IsStoreVariable(this Instruction.Types.OpCode opcode)
        {
            return opcode == Instruction.Types.OpCode.StoreVariable;
        }
    }

    public static class ValueOfOneCaseExt
    {
        public static bool IsNone(this Operand.ValueOneofCase type)
        {
            return Operand.ValueOneofCase.None == type;
        }

        public static bool IsStringValue(this Operand.ValueOneofCase type)
        {
            return Operand.ValueOneofCase.StringValue == type;
        }

        public static bool IsBoolValue(this Operand.ValueOneofCase type)
        {
            return Operand.ValueOneofCase.BoolValue == type;
        }

        public static bool IsFloatValue(this Operand.ValueOneofCase type)
        {
            return Operand.ValueOneofCase.FloatValue == type;
        }
    }
}

#endif