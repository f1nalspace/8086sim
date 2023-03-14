namespace CPU8086
{
    public readonly struct Mnemonic
    {
        public string Upper { get; }
        public string Lower { get; }

        public Mnemonic(string upper, string lower)
        {
            Upper = upper;
            Lower = lower;
        }

        public override string ToString() => Upper;
    }
}
