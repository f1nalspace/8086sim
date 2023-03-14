namespace Final.CPU8086
{
    public readonly struct AssemblyLine
    {
        public string Mnemonic { get; }
        public string Destination { get; }
        public string Source { get; }

        public AssemblyLine(string mnemonic, string destination = null, string source = null)
        {
            Mnemonic = mnemonic;
            Destination = destination;
            Source = source;
        }

        public AssemblyLine WithDestinationAndSource(string destination, string source)
            => new AssemblyLine(Mnemonic, destination, source);

        public AssemblyLine WithDestinationOnly(string destination)
            => new AssemblyLine(Mnemonic, destination, null);

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Destination) && !string.IsNullOrEmpty(Source))
                return $"{Mnemonic} {Destination}, {Source}";
            else if (!string.IsNullOrEmpty(Destination))
                return $"{Mnemonic} {Destination}";
            else
                return Mnemonic;
        }
    }
}
