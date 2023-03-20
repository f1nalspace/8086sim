namespace Final.CPU8086
{
    static class Mnemonics
    {
        public static readonly Mnemonic Push = new Mnemonic("PUSH");
        public static readonly Mnemonic Pop = new Mnemonic("POP");

        public static readonly Mnemonic Move = new Mnemonic("MOV");

        #region Arithmetic
        public static readonly Mnemonic ArithmeticAdd = new Mnemonic("ADD");
        public static readonly Mnemonic ArithmeticAddWithCarry = new Mnemonic("ADC");
        public static readonly Mnemonic ArithmeticSub = new Mnemonic("SUB");
        public static readonly Mnemonic ArithmeticSubWithBorrow = new Mnemonic("SBB");
        public static readonly Mnemonic ArithmeticCompare = new Mnemonic("CMP");
        #endregion

        #region Logical
        public static readonly Mnemonic LogicalOr = new Mnemonic("OR");
        public static readonly Mnemonic LogicalAnd = new Mnemonic("AND");
        public static readonly Mnemonic LogicalXor = new Mnemonic("XOR");
        #endregion

        #region Jumps
        public static readonly Mnemonic JumpAbove = new Mnemonic("JA");
        public static readonly Mnemonic JumpNotBelowOrEqual = new Mnemonic("JNBE");

        public static readonly Mnemonic JumpAboveOrEqual = new Mnemonic("JAE");
        public static readonly Mnemonic JumpOnNotBelow = new Mnemonic("JNB");
        
        public static readonly Mnemonic JumpBelow = new Mnemonic("JB");
        public static readonly Mnemonic JumpNotAboveOrEqual = new Mnemonic("JNAE");

        public static readonly Mnemonic JumpBelowOrEqual = new Mnemonic("JBE");
        public static readonly Mnemonic JumpNotAbove = new Mnemonic("JNA");

        public static readonly Mnemonic JumpOnCarry = new Mnemonic("JC");

        public static readonly Mnemonic JumpIfCXIsZero= new Mnemonic("JCXZ");
        public static readonly Mnemonic JumpIfECXIsZero = new Mnemonic("JECXZ");

        public static readonly Mnemonic JumpEqual = new Mnemonic("JE");
        public static readonly Mnemonic JumpZero = new Mnemonic("JZ");

        public static readonly Mnemonic JumpGreater = new Mnemonic("JG");
        public static readonly Mnemonic JumpNotLessOrEqual = new Mnemonic("JNLE");

        public static readonly Mnemonic JumpGreaterOrEqual = new Mnemonic("JGE");
        public static readonly Mnemonic JumpNotLess = new Mnemonic("JNL");

        public static readonly Mnemonic JumpLess = new Mnemonic("JL");
        public static readonly Mnemonic JumpNotGreaterOrEqual = new Mnemonic("JNGE");

        public static readonly Mnemonic JumpLessOrEqual = new Mnemonic("JLE");
        public static readonly Mnemonic JumpNotGreater = new Mnemonic("JNG");

        public static readonly Mnemonic JumpUnconditional = new Mnemonic("JMP");

        public static readonly Mnemonic JumpNotCarry = new Mnemonic("JNC");

        public static readonly Mnemonic JumpNotEqual = new Mnemonic("JNE");
        public static readonly Mnemonic JumpNotZero = new Mnemonic("JNZ");

        public static readonly Mnemonic JumpNotOverflow = new Mnemonic("JNO");

        public static readonly Mnemonic JumpNotSigned = new Mnemonic("JNS");

        public static readonly Mnemonic JumpNotParity = new Mnemonic("JNP");
        public static readonly Mnemonic JumpParityOdd = new Mnemonic("JPO");

        public static readonly Mnemonic JumpOnOverflow = new Mnemonic("JO");

        public static readonly Mnemonic JumpOnParity = new Mnemonic("JP");
        public static readonly Mnemonic JumpParityEven = new Mnemonic("JPE");

        public static readonly Mnemonic JumpSigned = new Mnemonic("JS");
        #endregion

        public static readonly Mnemonic Dynamic = new Mnemonic("(DYNAMIC)");
    }
}
