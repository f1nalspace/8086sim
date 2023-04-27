namespace Final.CPU8086.Execution
{
    public readonly struct ExecutedChange
    {
        public ExecutedValue Initial { get; }
        public ExecutedValue After { get; }

        public ExecutedChange(ExecutedValue initial, ExecutedValue after) : this()
        {
            Initial = initial;
            After = after;
        }

        public override string ToString() => $"'{Initial}' to '{After}'";
    }
}
