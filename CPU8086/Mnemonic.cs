using System;

namespace Final.CPU8086
{
    public readonly struct Mnemonic
    {
        public string Upper { get; }
        public string Lower { get; }

        public Mnemonic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            Upper = value.ToUpper();
            Lower = value.ToLower();
        }

        public override string ToString() => Upper;
    }
}
