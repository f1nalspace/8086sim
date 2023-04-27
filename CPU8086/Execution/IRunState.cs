using System.Collections.Generic;

namespace Final.CPU8086.Execution
{
    public interface IRunState
    {
        IReadOnlyCollection<ExecutedInstruction> Executed { get; }
        void AddExecuted(ExecutedInstruction executedInstruction);
    }
}
