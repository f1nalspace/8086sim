namespace Final.CPU8086
{
    public enum InstructionType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Ascii Adjust for Addition
        /// </summary>
        AAA,
        /// <summary>
        /// Ascii Adjust for Division
        /// </summary>
        AAD,
        /// <summary>
        /// Ascii Adjust for Multiplication
        /// </summary>
        AAM,
        /// <summary>
        /// Ascii Adjust for Subtraction
        /// </summary>
        AAS,
        /// <summary>
        /// Add With Carry
        /// </summary>
        ADC,
        /// <summary>
        /// Arithmetic Addition
        /// </summary>
        ADD,
        /// <summary>
        /// Logical And
        /// </summary>
        AND,
        /// <summary>
        /// Procedure Call
        /// </summary>
        CALL,
        /// <summary>
        /// Convert Byte to Word
        /// </summary>
        CBW,
        /// <summary>
        /// Clear Carry
        /// </summary>
        CLC,
        /// <summary>
        /// Clear Direction Flag
        /// </summary>
        CLD,
        /// <summary>
        /// Clear Interrupt Flag (Disable Interrupts)
        /// </summary>
        CLI,
        /// <summary>
        /// Complement Carry Flag
        /// </summary>
        CMC,
        /// <summary>
        /// Compare
        /// </summary>
        CMP,
        /// <summary>
        /// Compare String (Byte, Word or Doubleword)
        /// </summary>
        CMPS,
        /// <summary>
        /// Convert Word to Doubleword
        /// </summary>
        CWD,
        /// <summary>
        /// Decimal Adjust for Addition
        /// </summary>
        DAA,
        /// <summary>
        /// Decimal Adjust for Subtraction
        /// </summary>
        DAS,
        /// <summary>
        /// Decrement
        /// </summary>
        DEC,
        /// <summary>
        /// Divide
        /// </summary>
        DIV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        F2XM1,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FABS,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FADD,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FADDP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FBLD,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FBSTP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FCHS,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FCLEX,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FCOM,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FCOMP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FCOMPP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDECSTP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDISI,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDIV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDIVP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDIVR,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FDIVRP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FENI,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FFREE,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FIADD,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FICOM,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FICOMP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FIDIV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FIDIVR,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FILD,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FIMUL,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FINCSTP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FINIT,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FIST,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FISTP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FISUB,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FISUBR,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLD,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLD1,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDCW,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDENV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDL2E,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDL2T,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDLG2,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDLN2,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDPI,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FLDZ,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FMUL,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FMULP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNCLEX,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNDISI,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNENI,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNINIT,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNOP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNSAVE,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNSTCW,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNSTENV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FNSTSW,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FPATAN,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FPREM,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FPTAN,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FRNDINT,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FRSTOR,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSAVE,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSCALE,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSETPM,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSQRT,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FST,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSTCW,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSTENV,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSTP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSTSW,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSUB,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSUBP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSUBR,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FSUBRP,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FTST,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FXAM,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FXCH,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FXTRACT,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FYL2X,
        /// <summary>
        /// A description of the floating point instructions is not available at yet.
        /// </summary>
        FYL2XP1,
        /// <summary>
        /// Halt CPU
        /// </summary>
        HLT,
        /// <summary>
        /// Signed Integer Division
        /// </summary>
        IDIV,
        /// <summary>
        /// Input Byte or Word From Port
        /// </summary>
        IN,
        /// <summary>
        /// Increment
        /// </summary>
        INC,
        /// <summary>
        /// Interrupt
        /// </summary>
        INT,
        /// <summary>
        /// Interrupt on Overflow
        /// </summary>
        INTO,
        /// <summary>
        /// Interrupt Return
        /// </summary>
        IRET,
        /// <summary>
        /// Jump if Register (E)CX is Zero
        /// </summary>
        JCXZ,
        /// <summary>
        /// Unconditional Jump
        /// </summary>
        JMP,
        /// <summary>
        /// Load Register AH From Flags
        /// </summary>
        LAHF,
        /// <summary>
        /// Load Pointer Using DS
        /// </summary>
        LDS,
        /// <summary>
        /// Load Effective Address
        /// </summary>
        LEA,
        /// <summary>
        /// Load Pointer Using ES
        /// </summary>
        LES,
        /// <summary>
        /// Lock Bus
        /// </summary>
        LOCK,
        /// <summary>
        /// Load String (Byte, Word or Double)
        /// </summary>
        LODS,
        /// <summary>
        /// Decrement CX and Loop if CX Not Zero
        /// </summary>
        LOOP,
        /// <summary>
        /// Loop While Equal / Loop While Zero
        /// </summary>
        LOOPE,
        /// <summary>
        /// Loop While Not Zero / Loop While Not Equal
        /// </summary>
        LOOPNZ,
        /// <summary>
        /// Move Byte or Word
        /// </summary>
        MOV,
        /// <summary>
        /// Move String (Byte or Word)
        /// </summary>
        MOVS,
        /// <summary>
        /// Unsigned Multiply
        /// </summary>
        MUL,
        /// <summary>
        /// Two's Complement Negation
        /// </summary>
        NEG,
        /// <summary>
        /// No Operation
        /// </summary>
        NOP,
        /// <summary>
        /// One's Compliment Negation (Logical NOT)
        /// </summary>
        NOT,
        /// <summary>
        /// Inclusive Logical OR
        /// </summary>
        OR,
        /// <summary>
        /// Output Data to Port
        /// </summary>
        OUT,
        /// <summary>
        /// Pop Word off Stack
        /// </summary>
        POP,
        /// <summary>
        /// Pop Flags off Stack
        /// </summary>
        POPF,
        /// <summary>
        /// Push Word onto Stack
        /// </summary>
        PUSH,
        /// <summary>
        /// Push Flags onto Stack
        /// </summary>
        PUSHF,
        /// <summary>
        /// Rotate Through Carry Left
        /// </summary>
        RCL,
        /// <summary>
        /// Rotate Through Carry Right
        /// </summary>
        RCR,
        /// <summary>
        /// Repeat String Operation
        /// </summary>
        REP,
        /// <summary>
        /// Repeat Equal / Repeat Zero
        /// </summary>
        REPE,
        /// <summary>
        /// Repeat Not Equal / Repeat Not Zero
        /// </summary>
        REPNE,
        /// <summary>
        /// Return From Procedure
        /// </summary>
        RET,
        /// <summary>
        /// Rotate Left
        /// </summary>
        ROL,
        /// <summary>
        /// Rotate Right
        /// </summary>
        ROR,
        /// <summary>
        /// Store AH Register into FLAGS
        /// </summary>
        SAHF,
        /// <summary>
        /// Shift Arithmetic Left / Shift Logical Left
        /// </summary>
        SAL,
        /// <summary>
        /// Shift Arithmetic Right
        /// </summary>
        SAR,
        /// <summary>
        /// Subtract with Borrow
        /// </summary>
        SBB,
        /// <summary>
        /// Scan String (Byte, Word or Doubleword)
        /// </summary>
        SCAS,
        /// <summary>
        /// Shift Logical Right
        /// </summary>
        SHR,
        /// <summary>
        /// Set Carry
        /// </summary>
        STC,
        /// <summary>
        /// Set Direction Flag
        /// </summary>
        STD,
        /// <summary>
        /// Set Interrupt Flag (Enable Interrupts)
        /// </summary>
        STI,
        /// <summary>
        /// Store String (Byte, Word or Doubleword)
        /// </summary>
        STOS,
        /// <summary>
        /// Subtract
        /// </summary>
        SUB,
        /// <summary>
        /// Test For Bit Pattern
        /// </summary>
        TEST,
        /// <summary>
        /// Event Wait
        /// </summary>
        WAIT,
        /// <summary>
        /// Exchange
        /// </summary>
        XCHG,
        /// <summary>
        /// Translate
        /// </summary>
        XLAT,
        /// <summary>
        /// Exclusive OR
        /// </summary>
        XOR,
    }
}
