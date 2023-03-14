namespace CPU8086
{
    static class Mnemonics
    {
        public static readonly Mnemonic Push = new Mnemonic("PUSH", "push");
        public static readonly Mnemonic Pop = new Mnemonic("POP", "pop");

        public static readonly Mnemonic Mov = new Mnemonic("MOV", "mov");

        public static readonly Mnemonic Add = new Mnemonic("ADD", "add");
        public static readonly Mnemonic Or = new Mnemonic("OR", "or");
        public static readonly Mnemonic AddWithCarry = new Mnemonic("ADC", "adc");
        public static readonly Mnemonic SubWithBorrow = new Mnemonic("SBB", "sbb");
        public static readonly Mnemonic And = new Mnemonic("AND", "and");
        public static readonly Mnemonic Sub = new Mnemonic("SUB", "sub");
        public static readonly Mnemonic Xor = new Mnemonic("XOR", "xor");
        public static readonly Mnemonic Cmp = new Mnemonic("CMP", "cmp");

        public static readonly Mnemonic Dynamic = new Mnemonic("(DYNAMIC)", "(dynamic)");
    }
}
