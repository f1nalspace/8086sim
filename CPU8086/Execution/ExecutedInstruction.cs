using Final.CPU8086.Instructions;
using System;

namespace Final.CPU8086.Execution
{
    public class ExecutedInstruction
    {
        public Instruction Instruction { get; }
        public ExecutedChange[] Changes { get; }

        public ExecutedInstruction(Instruction instruction, params ExecutedChange[] changes)
        {
            Instruction = instruction;
            Changes = changes ?? Array.Empty<ExecutedChange>();
        }
    }
}
