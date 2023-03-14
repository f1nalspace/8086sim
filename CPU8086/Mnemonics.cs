namespace Final.CPU8086
{
    static class Mnemonics
    {
        public static readonly Mnemonic Push = new Mnemonic("PUSH", "push");
        public static readonly Mnemonic Pop = new Mnemonic("POP", "pop");

        public static readonly Mnemonic Mov = new Mnemonic("MOV", "mov");

        public static readonly Mnemonic Addition = new Mnemonic("ADD", "add");
        public static readonly Mnemonic Or = new Mnemonic("OR", "or");
        public static readonly Mnemonic AddWithCarry = new Mnemonic("ADC", "adc");
        public static readonly Mnemonic SubtractWithBorrow = new Mnemonic("SBB", "sbb");
        public static readonly Mnemonic And = new Mnemonic("AND", "and");
        public static readonly Mnemonic Subtrace = new Mnemonic("SUB", "sub");
        public static readonly Mnemonic Xor = new Mnemonic("XOR", "xor");
        public static readonly Mnemonic Compare = new Mnemonic("CMP", "cmp");

        public static readonly Mnemonic Dynamic = new Mnemonic("(DYNAMIC)", "(dynamic)");
    }
}
