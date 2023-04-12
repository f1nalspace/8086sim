namespace Final.CPU8086
{
    public enum AssemblyLineType
    {
        Default = 0,
        SourceLabel,
        TargetLabel
    }

    public readonly struct AssemblyLine
    {
        public uint Position { get; }
        public AssemblyLineType Type { get; }
        public Mnemonic Mnemonic { get; }
        public string Assembly { get; }
        public string Label { get; }

        public AssemblyLine(uint position, AssemblyLineType type, Mnemonic mnemonic, string assembly, string label)
        {
            Position = position;
            Type = type;
            Mnemonic = mnemonic;
            Assembly = assembly;
            Label = label;
        }

        public override string ToString()
        {
            if (Type == AssemblyLineType.SourceLabel)
                return $"{Label}:";
            else if (Type == AssemblyLineType.TargetLabel)
                return $"{Mnemonic} {Label}";
            else
                return $"{Mnemonic} {Assembly}";
        }
    }
}
