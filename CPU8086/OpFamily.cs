namespace Final.CPU8086
{
    // @TODO(final): Remove OpFamily
    public enum OpFamily : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Decrements the <see cref="RegisterType.SP"/> by the value of a fixed register
        /// </summary>
        Push_FixedReg,
        /// <summary>
        /// Increments the <see cref="RegisterType.SP"/> by the value of a fixed register
        /// </summary>
        Pop_FixedReg,

        /// <summary>
        /// Move 8-bit register/memory to 8-bit register/memory (MOV REG8/MEM8, REG8/MEM8)
        /// </summary>
        Move8_RegOrMem_RegOrMem,
        /// <summary>
        /// Move 16-bit register/memory to 16-bit register/memory (MOV REG16/MEM16, REG16/MEM16)
        /// </summary>
        Move16_RegOrMem_RegOrMem,

        /// <summary>
        /// Move 8-bit immediate to 8-bit register (MOV REG8, IMM8)
        /// </summary>
        Move8_Reg_Imm,
        /// <summary>
        /// Move 16-bit immediate to 16-bit register (MOV REG16, IMM16)
        /// </summary>
        Move16_Reg_Imm,

        /// <summary>
        /// Move 8-bit immediate to 8-bit memory (MOV MEM8, IMM8)
        /// </summary>
        Move8_Mem_Imm,
        /// <summary>
        /// Move 16-bit immediate to 16-bit memory (MOV MEM16, IMM16)
        /// </summary>
        Move16_Mem_Imm,

        /// <summary>
        /// Move 8-bit memory to 8-bit fixed register (MOV AL, MEM8)
        /// </summary>
        Move8_FixedReg_Mem,
        /// <summary>
        /// Move 16-bit memory to 16-bit fixed register (MOV AX, MEM16)
        /// </summary>
        Move16_FixedReg_Mem,

        /// <summary>
        /// Move 8-bit fixed register to 168bit memory (MOV MEM8, AL)
        /// </summary>
        Move8_Mem_FixedReg,
        /// <summary>
        /// Move 16-bit fixed register to 16-bit memory (MOV MEM16, AX)
        /// </summary>
        Move16_Mem_FixedReg,

        /// <summary>
        /// Add 8-bit register to 8-bit register/memory (ADD REG8/MEM8, REG8)
        /// </summary>
        Add8_RegOrMem_RegOrMem,
        /// <summary>
        /// Add 16-bit register to 16-bit register/memory (ADD REG16/MEM16, REG16)
        /// </summary>
        Add16_RegOrMem_RegOrMem,
        /// <summary>
        /// Add 8-bit immediate to 8-bit fixed register (ADD AL, IMM8)
        /// </summary>
        Add8_FixedReg_Imm,
        /// <summary>
        /// Add 16-bit immediate to 16-bit fixed register (ADD AX, IMM8)
        /// </summary>
        Add16_FixedReg_Imm,

        /// <summary>
        /// Logical OR to 8-bit register/memory with 8-bit immediate (OR AL, BH)
        /// </summary>
        Or8_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical OR to 16-bit register/memory with 16-bit immediate (OR AX, BX)
        /// </summary>
        Or16_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical OR to 8-bit fixed register with 8-bit immediate (OR AL, IMM8)
        /// </summary>
        Or8_FixedReg_Imm,
        /// <summary>
        /// Logical OR to 16-bit fixed register with 16-bit immediate (OR AX, IMM16)
        /// </summary>
        Or16_FixedReg_Imm,

        /// <summary>
        /// Add with carry 8-bit register/memory to 8-bit register/memory (ADC AL, BL)
        /// </summary>
        Adc8_RegOrMem_RegOrMem,
        /// <summary>
        /// Add with carry 16-bit register/memory to 16-bit register/memory (ADC AX, BX)
        /// </summary>
        Adc16_RegOrMem_RegOrMem,
        /// <summary>
        /// Add with carry 8-bit immediate to 8-bit fixed register
        /// </summary>
        Adc8_FixedReg_Imm,
        /// <summary>
        /// Add with carry 16-bit immediate to 16-bit fixed register
        /// </summary>
        Adc16_FixedReg_Imm,

        /// <summary>
        /// Sub with borrow 8-bit register/memory from 8-bit register/memory (ADC AL, BL)
        /// </summary>
        Sbb8_RegOrMem_RegOrMem,
        /// <summary>
        /// Sub with borrow 16-bit register/memory from 16-bit register/memory (ADC AX, BX)
        /// </summary>
        Sbb16_RegOrMem_RegOrMem,
        /// <summary>
        /// Sub with borrow 8-bit immediate from 8-bit fixed register
        /// </summary>
        Sbb8_FixedReg_Imm,
        /// <summary>
        /// Sub with borrow 16-bit immediate from 16-bit fixed register
        /// </summary>
        Sbb16_FixedReg_Imm,

        /// <summary>
        /// Logical AND 8-bit register/memory with 8-bit register/memory (ADC AL, BL)
        /// </summary>
        And8_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical AND 16-bit register/memory with 16-bit register/memory (ADC AX, BX)
        /// </summary>
        And16_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical AND 8-bit immediate with 8-bit fixed register
        /// </summary>
        And8_FixedReg_Imm,
        /// <summary>
        /// Logical AND 16-bit immediate with 16-bit fixed register
        /// </summary>
        And16_FixedReg_Imm,

        /// <summary>
        /// Subtract 8-bit register/memory from 8-bit register/memory (ADC AL, BL)
        /// </summary>
        Sub8_RegOrMem_RegOrMem,
        /// <summary>
        /// Subtract 16-bit register/memory from 16-bit register/memory (ADC AX, BX)
        /// </summary>
        Sub16_RegOrMem_RegOrMem,
        /// <summary>
        /// Subtract 8-bit immediate from 8-bit fixed register
        /// </summary>
        Sub8_FixedReg_Imm,
        /// <summary>
        /// Subtract 16-bit immediate from 16-bit fixed register
        /// </summary>
        Sub16_FixedReg_Imm,

        /// <summary>
        /// Logical XOR 8-bit register/memory with 8-bit register/memory (ADC AL, BL)
        /// </summary>
        Xor8_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical XOR 16-bit register/memory with 16-bit register/memory (ADC AX, BX)
        /// </summary>
        Xor16_RegOrMem_RegOrMem,
        /// <summary>
        /// Logical XOR 8-bit immediate with 8-bit fixed register
        /// </summary>
        Xor8_FixedReg_Imm,
        /// <summary>
        /// Logical XOR 16-bit immediate with 16-bit fixed register
        /// </summary>
        Xor16_FixedReg_Imm,

        /// <summary>
        /// Compare 8-bit register/memory with 8-bit register/memory (ADC AL, BL)
        /// </summary>
        Cmp8_RegOrMem_RegOrMem,
        /// <summary>
        /// Compare 16-bit register/memory with 16-bit register/memory (ADC AX, BX)
        /// </summary>
        Cmp16_RegOrMem_RegOrMem,
        /// <summary>
        /// Compare 8-bit immediate with 8-bit fixed register
        /// </summary>
        Cmp8_FixedReg_Imm,
        /// <summary>
        /// Compare 16-bit immediate with 16-bit fixed register
        /// </summary>
        Cmp16_FixedReg_Imm,

        /// <summary>
        /// <para>Arithmetic operation 8-bit immediate to 8-bit register/memory (ADD REG8/MEM8, IMM8).</para>
        /// <para>REG field decides which mathmatical instruction it is:</para>
        /// <para>REG (000) = ADD</para>
        /// <para>REG (001) = OR</para>
        /// <para>REG (010) = ADC</para>
        /// <para>REG (011) = SBB</para>
        /// <para>REG (100) = AND</para>
        /// <para>REG (101) = SUB</para>
        /// <para>REG (110) = XOR</para>
        /// <para>REG (111) = CMP</para>
        /// </summary>
        Arithmetic8_RegOrMem_Imm,

        /// <summary>
        /// <para>Arithmetic operation 16-bit immediate to 16-bit register/memory (ADD REG16/MEM16, IMM16).</para>
        /// <para>REG field decides which mathmatical instruction it is:</para>
        /// <para>REG (000) = ADD</para>
        /// <para>REG (001) = Not used</para>
        /// <para>REG (010) = ADC</para>
        /// <para>REG (011) = SBB</para>
        /// <para>REG (100) = Not used</para>
        /// <para>REG (101) = SUB</para>
        /// <para>REG (110) = Not used</para>
        /// <para>REG (111) = CMP</para>
        /// </summary>
        Arithmetic16_RegOrMem_Imm,

        /// <summary>
        /// Jump when not equal
        /// </summary>
        Jump8,
    }
}
