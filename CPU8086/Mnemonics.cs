namespace Final.CPU8086
{
    static class Mnemonics
    {
        public static readonly Mnemonic Push = new Mnemonic("PUSH", "push");
        public static readonly Mnemonic Pop = new Mnemonic("POP", "pop");

        public static readonly Mnemonic Move = new Mnemonic("MOV", "mov");

        public static readonly Mnemonic ArithmeticAdd = new Mnemonic("ADD", "add");
        public static readonly Mnemonic ArithmeticAddWithCarry = new Mnemonic("ADC", "adc");
        public static readonly Mnemonic ArithmeticSub = new Mnemonic("SUB", "sub");
        public static readonly Mnemonic ArithmeticSubWithBorrow = new Mnemonic("SBB", "sbb");
        public static readonly Mnemonic ArithmeticCompare = new Mnemonic("CMP", "cmp");
        public static readonly Mnemonic LogicalOr = new Mnemonic("OR", "or");
        public static readonly Mnemonic LogicalAnd = new Mnemonic("AND", "and");
        public static readonly Mnemonic LogicalXor = new Mnemonic("XOR", "xor");

        public static readonly Mnemonic Dynamic = new Mnemonic("(DYNAMIC)", "(dynamic)");
    }
}
