namespace Final.CPU8086.Instructions
{
    public enum InstructionType : byte
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
        CMPSB,
        /// <summary>
        /// Compare String (Byte, Word or Doubleword)
        /// </summary>
        CMPSW,
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
        /// Signed Multiply
        /// </summary>
        IMUL,
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
        /// Jump Above / Jump Not Below or Equal
        /// </summary>
        JA,
        /// <summary>
        /// Jump Above or Equal / Jump on Not Below
        /// </summary>
        JAE,
        /// <summary>
        /// Jump Below / Jump Not Above or Equal
        /// </summary>
        JB,
        /// <summary>
        /// Jump Below or Equal / Jump Not Above
        /// </summary>
        JBE,
        /// <summary>
        /// Jump on Carry
        /// </summary>
        JC,
        /// <summary>
        /// Jump if Register (E)CX is Zero
        /// </summary>
        JCXZ,
        /// <summary>
        /// Jump Equal / Jump Zero
        /// </summary>
        JE,
        /// <summary>
        /// Jump Greater / Jump Not Less or Equal
        /// </summary>
        JG,
        /// <summary>
        /// Jump Greater or Equal / Jump Not Less
        /// </summary>
        JGE,
        /// <summary>
        /// Jump Less / Jump Not Greater or Equal
        /// </summary>
        JL,
        /// <summary>
        /// Jump Less or Equal / Jump Not Greater
        /// </summary>
        JLE,
        /// <summary>
        /// Unconditional Jump
        /// </summary>
        JMP,
        /// <summary>
        /// Jump Not Carry
        /// </summary>
        JNC,
        /// <summary>
        /// Jump Not Equal / Jump Not Zero
        /// </summary>
        JNE,
        /// <summary>
        /// Jump Not Overflow
        /// </summary>
        JNO,
        /// <summary>
        /// Jump Not Signed
        /// </summary>
        JNS,
        /// <summary>
        /// Jump Not Parity / Jump Parity Odd
        /// </summary>
        JNP,
        /// <summary>
        /// Jump on Overflow
        /// </summary>
        JO,
        /// <summary>
        /// Jump on Parity / Jump on Parity Even
        /// </summary>
        JP,
        /// <summary>
        /// Jump Signed
        /// </summary>
        JS,
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
        /// Load String (Byte, Word or Double)
        /// </summary>
        LODSB,
        /// <summary>
        /// Load String (Byte, Word or Double)
        /// </summary>
        LODSW,
        /// <summary>
        /// Loop While Not Zero (CX == 0)
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
        MOVSB,
        /// <summary>
        /// Move String (Byte or Word)
        /// </summary>
        MOVSW,
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
        /// Repeat Equal / Repeat Zero
        /// </summary>
        REPE,
        /// <summary>
        /// Return From Procedure
        /// </summary>
        RET,
        /// <summary>
        /// Return From Procedure
        /// </summary>
        RETF,
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
        /// Shift Arithmetic Left / Shift Logical Left
        /// </summary>
        SHL,
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
        SCASB,
        /// <summary>
        /// Scan String (Byte, Word or Doubleword)
        /// </summary>
        SCASW,
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
        STOSB,
        /// <summary>
        /// Store String (Byte, Word or Doubleword)
        /// </summary>
        STOSW,
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
        /// <summary>
        /// Lock Prefix
        /// </summary>
        LOCK,
        /// <summary>
        /// Repeat Not Equal Prefix
        /// </summary>
        REPNE,
        /// <summary>
        /// Repeat Prefix
        /// </summary>
        REP,
        /// <summary>
        /// CS Segment Override Prefix
        /// </summary>
        CS,
        /// <summary>
        /// SS Segment Override Prefix
        /// </summary>
        SS,
        /// <summary>
        /// DS Segment Override Prefix
        /// </summary>
        DS,
        /// <summary>
        /// ES Segment Override Prefix
        /// </summary>
        ES,
        /// <summary>
        /// FS Segment Override Prefix
        /// </summary>
        FS,
        /// <summary>
        /// GS Segment Override Prefix
        /// </summary>
        GS,
        /// <summary>
        /// Data to 8-bit Override Prefix
        /// </summary>
        DATA8,
        /// <summary>
        /// Data to 16-bit Override Prefix
        /// </summary>
        DATA16,
        /// <summary>
        /// Address to 8-bit Override Prefix
        /// </summary>
        ADDR8,
        /// <summary>
        /// Address to 16-bit Override Prefix
        /// </summary>
        ADDR16,
    }
}
