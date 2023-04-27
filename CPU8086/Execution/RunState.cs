using System.Collections.Generic;

namespace Final.CPU8086.Execution
{
    public class RunState : IRunState
    {
        public bool IsStopped { get; set; } = false;

        IReadOnlyCollection<ExecutedInstruction> IRunState.Executed => _executed;
        private readonly List<ExecutedInstruction> _executed = new List<ExecutedInstruction>();

        void IRunState.AddExecuted(ExecutedInstruction executedInstruction) => _executed.Add(executedInstruction);
    }
}
