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
